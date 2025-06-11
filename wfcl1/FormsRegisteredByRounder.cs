using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CuoreUI.Components;

internal static class FormsRegisteredByRounder
{
    public class RegisteredForm
    {
        public cuiFormRounder rounder;
        public Form targetForm;
    }

    static List<RegisteredForm> registeredFormList = new List<RegisteredForm>();

    public static int registeredCount => registeredFormList.Count;

    public static void RemoveByForm(Form formToRemove)
    {
        registeredFormList = registeredFormList.Where(f => f.targetForm != formToRemove).ToList();
    }

    public static bool AddByForm(Form formToAdd, cuiFormRounder rounderToAdd)
    {
        if (GetRounderByForm(formToAdd) != null)
        {
            return false;
        }

        RegisteredForm registeredForm = new RegisteredForm()
        {
            targetForm = formToAdd,
            rounder = rounderToAdd
        };

        registeredFormList.Add(registeredForm);

        return true;
    }

    public static cuiFormRounder GetRounderByForm(Form formSelector)
    {
        RegisteredForm foundForm = registeredFormList.FirstOrDefault(f => f.targetForm == formSelector);
        if (foundForm != null) 
        {
            return foundForm.rounder;
        }

        return null;
    }
}
