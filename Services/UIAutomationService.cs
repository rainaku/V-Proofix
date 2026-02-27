using System;
using System.Windows.Automation;

namespace VProofix.Services
{
    public class UIAutomationService
    {
        public string GetTextFromFocusedElement()
        {
            try
            {
                var element = AutomationElement.FocusedElement;
                if (element == null) return string.Empty;

                // Try TextPattern (common for rich text boxes, VS Code, Word, etc.)
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out object textPatternObj))
                {
                    TextPattern textPattern = (TextPattern)textPatternObj;
                    var selections = textPattern.GetSelection();
                    if (selections != null && selections.Length > 0)
                    {
                        return selections[0].GetText(-1);
                    }
                }

                // Try ValuePattern (common for standard text boxes)
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object valuePatternObj))
                {
                    ValuePattern valuePattern = (ValuePattern)valuePatternObj;
                    return valuePattern.Current.Value; // Note: this might return the whole text, not just selection
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
