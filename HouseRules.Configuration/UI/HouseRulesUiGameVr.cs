﻿namespace HouseRules.Configuration.UI
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Common.UI;
    using Common.UI.Element;
    using HouseRules.Core;
    using Revolutions;
    using UnityEngine;

    internal class HouseRulesUiGameVr : MonoBehaviour
    {
        private VrResourceTable _resourceTable;
        private IElementCreator _elementCreator;
        private Transform _anchor;

        private void Start()
        {
            StartCoroutine(WaitAndInitialize());
        }

        private IEnumerator WaitAndInitialize()
        {
            while (!VrElementCreator.IsReady()
                   || Resources
                       .FindObjectsOfTypeAll<GameObject>()
                       .Count(x => x.name == "~LeanTween") < 1)
            {
                HouseRulesConfigurationBase.LogDebug("UI dependencies not yet ready. Waiting...");
                yield return new WaitForSecondsRealtime(1);
            }

            HouseRulesConfigurationBase.LogDebug("UI dependencies ready. Proceeding with initialization.");

            _resourceTable = VrResourceTable.Instance();
            _elementCreator = VrElementCreator.Instance();
            _anchor = Resources
                .FindObjectsOfTypeAll<GameObject>()
                .First(x => x.name == "~LeanTween").transform;

            Initialize();
            HouseRulesConfigurationBase.LogDebug("Initialization complete.");
        }

        private void Initialize()
        {
            transform.SetParent(_anchor, worldPositionStays: true);
            transform.position = new Vector3(38f, 41.4f, -22f);
            if (HR.SelectedRuleset.Name.Contains("Demeo Revolutions") || HR.SelectedRuleset.Name.Equals("TEST GAME"))
            {
                transform.rotation = Quaternion.Euler(0, 90, 0);
            }
            else
            {
                gameObject.AddComponent<FaceLocalPlayer>();
            }

            int numRules = 11;
            int textLength = 0;
            int returnCount = 0;
            if (!string.IsNullOrEmpty(HR.SelectedRuleset.Longdesc))
            {
                textLength = HR.SelectedRuleset.Longdesc.Length;
            }

            if (textLength < 1)
            {
                numRules = HR.SelectedRuleset.Rules.Count;
            }
            else
            {
                // Count how many return characters are in the string
                returnCount = HR.SelectedRuleset.Longdesc.Count(f => f == '\n');
                foreach (Match match in Regex.Matches(HR.SelectedRuleset.Longdesc, "<color=", RegexOptions.None))
                {
                    // Subtract colorization code from textLength
                    textLength -= 23;
                }

                if (textLength > 975)
                {
                    // HouseRulesConfigurationBase.LogDebug($"{(textLength - 650) / 65} from text of {textLength}");
                    numRules += 3 + ((textLength - 650) / 65);
                    if (returnCount > 11)
                    {
                        // HouseRulesConfigurationBase.LogDebug($"{returnCount - 11} from returns");
                        numRules += returnCount - 11;
                    }
                }
                else if (returnCount > 11)
                {
                    // HouseRulesConfigurationBase.LogDebug($"{returnCount - 11} from JUST returns");
                    numRules += returnCount - 11;
                }
                else if (returnCount + (textLength / (25 * returnCount)) > 11)
                {
                    // HouseRulesConfigurationBase.LogDebug($"{returnCount + (textLength / (25 * returnCount)) - 11} from returns and text combined");
                    numRules += returnCount + (textLength / (25 * returnCount)) - 11;
                }
            }

            // HouseRulesConfigurationBase.LogDebug($"returnCount: {returnCount} numRules: {numRules}");
            var background = new GameObject("Background");
            var scale = 1.5f;
            background.AddComponent<MeshFilter>().mesh = _resourceTable.MenuMesh;
            background.AddComponent<MeshRenderer>().material = _resourceTable.MenuMaterial;
            background.transform.SetParent(transform, worldPositionStays: false);
            background.transform.localPosition = new Vector3(0, 0, 0);
            background.transform.localRotation =
                Quaternion.Euler(-90, 0, 0); // Un-flip card from it's default face-up position.
            if (numRules > 11)
            {
                scale += (float)(0.09 * (numRules - 11));
                if (scale > 10f)
                {
                    scale = 10f;
                    numRules = 110;
                }
            }

            background.transform.localScale = new Vector3(4.75f, 1, scale);

            var header = 3.6f;
            var headerText = _elementCreator.CreateMenuHeaderText("<b>Frakolutions</b>");
            headerText.transform.SetParent(transform, worldPositionStays: false);
            if (numRules > 11)
            {
                header = 3.6f + (float)(0.21f * (numRules - 11));
            }

            headerText.transform.localPosition = new Vector3(0, header, VrElementCreator.TextZShift);
            Color indigo = new Color(0.294f, 0f, 0.51f);
            Color brown = new Color(0.0392f, 0.0157f, 0, 1);
            var sb = new StringBuilder();
            sb.Append(ColorizeString("Playing ", brown));
            sb.Append(ColorizeString($"{HR.SelectedRuleset.Name}", indigo));
            sb.AppendLine(ColorizeString(" ruleset!", brown));
            sb.AppendLine(ColorizeString($"{HR.SelectedRuleset.Description}", Color.blue));
            sb.AppendLine();
            float ruleset = 0;
            float drift = 1.25f;
            var rulesetPanel = _elementCreator.CreateNewText(sb.ToString());
            rulesetPanel.transform.SetParent(transform, worldPositionStays: false);
            if (numRules > 40)
            {
                drift -= (float)(0.035f * (numRules - 20));
            }
            else if (numRules > 20)
            {
                drift -= (float)(0.045f * (numRules - 20));
            }

            if (numRules > 11)
            {
                ruleset = drift + (float)(0.2f * (numRules - 11));
            }
            else
            {
                ruleset = drift + 0.4f;
            }

            rulesetPanel.transform.localPosition = new Vector3(0, ruleset, VrElementCreator.TextZShift);

            sb.Clear();
            if (textLength > 0)
            {
                //sb.AppendLine(ColorizeString("<========== Ruleset Creator's Description ==========>", Color.white));
                sb.AppendLine(ColorizeString($"{HR.SelectedRuleset.Longdesc}", Color.black));
            }
            else
            {
                int total = 0;
                if (numRules > 1)
                {
                    sb.AppendLine(ColorizeString($"<========== {numRules} Active Rules ==========>", Color.white));
                }
                else
                {
                    sb.AppendLine(ColorizeString("<========== 1 Active Rule ==========>", Color.white));
                }

                foreach (var rule in HR.SelectedRuleset.Rules)
                {
                    try
                    {
                        var description = HR.SelectedRuleset.Rules[total].Description;
                        sb.AppendLine(ColorizeString($"- {description}", Color.black));
                        total++;
                    }
                    catch (Exception e)
                    {
                        // TODO(orendain): Consider rolling back or disable rule.
                        HouseRulesConfigurationBase.LogWarning($"Failed to successfully call on rule [{rule.GetType()}]: {e}");
                    }
                }
            }

            var center = -2f + (float)(numRules * .05);
            var details = center - (float)(0.245 * numRules);
            var detailsPanel = _elementCreator.CreateLeftText(sb.ToString());
            detailsPanel.transform.SetParent(transform, worldPositionStays: false);
            detailsPanel.transform.localPosition = new Vector3(0, details, VrElementCreator.TextZShift);

            sb.Clear();
            sb.Append(ColorizeString($"v{RevolutionsVersion.Version}", Color.yellow));
            var version = -9.5f;
            var versionText = _elementCreator.CreateNormalText(sb.ToString());
            versionText.transform.SetParent(transform, worldPositionStays: false);
            if (numRules > 11)
            {
                version = -9.5f - (float)(0.58 * (numRules - 11));
            }

            versionText.transform.localPosition = new Vector3(-7, version, VrElementCreator.TextZShift);

            if (HouseRulesConfigurationBase.IsUpdateAvailable)
            {
                sb.Clear();
                sb.Append(ColorizeString("NEW UPDATE AVAILABLE", Color.green));
                var updateText = _elementCreator.CreateNormalText(sb.ToString());
                updateText.transform.SetParent(transform, worldPositionStays: false);
                updateText.transform.localPosition = new Vector3(5.75f, version, VrElementCreator.TextZShift);
            }

            // TODO(orendain): Fix so that ray interacts with entire object.
            gameObject.AddComponent<BoxCollider>();
        }

        private static string ColorizeString(string text, Color color)
        {
            return string.Concat(new string[]
            {
        " <color=#",
        ColorUtility.ToHtmlStringRGB(color),
        ">",
        text,
        "</color>",
            });
        }
    }
}
