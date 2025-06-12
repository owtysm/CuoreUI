using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CuoreUI.Properties;

namespace CuoreUI.Controls
{
    [Description("Card company's logo shows as the user inputs their credit card number")]
    [DefaultEvent("IsCardValidChanged")]
    public partial class cuiTextBoxCreditCardNumber : cuiTextBox
    {
        public event EventHandler IsCardValidChanged;

        public cuiTextBoxCreditCardNumber()
        {
            InitializeComponent();
            ContentChanged += CuiTextBoxCreditCardNumber_ContentChanged;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Image Image { get => base.Image; private set => base.Image = value; }

        private void FormatCreditCardSoFar()
        {
            int caretPositionBeforeFormat = contentTextField.SelectionStart;

            string cleaned = new string(actualText.Where(char.IsDigit).Take(19).ToArray());

            string formatted = string.Join(" ", Enumerable.Range(0, (cleaned.Length + 3) / 4)
                .Select(i => cleaned.Substring(i * 4, Math.Min(4, cleaned.Length - i * 4))));

            // only set if different from current
            if (actualText != formatted)
            {
                int spacesBeforeCaret = actualText.Substring(0, Math.Min(caretPositionBeforeFormat, actualText.Length)).Count(c => c == ' ');
                int newCaretPosition = caretPositionBeforeFormat - spacesBeforeCaret;

                int fixedCaret = 0;
                int characters = 0;
                foreach (var part in formatted.Split(' '))
                {
                    if (characters + part.Length >= newCaretPosition)
                    {
                        fixedCaret += newCaretPosition - characters;
                        break;
                    }
                    fixedCaret += part.Length + 1; // +1 for space
                    characters += part.Length;
                }

                contentTextField.Text = formatted;
                contentTextField.SelectionStart = Math.Min(fixedCaret, contentTextField.Text.Length);
            }
        }

        private void CuiTextBoxCreditCardNumber_ContentChanged(object sender, EventArgs e)
        {
            FormatCreditCardSoFar();

            CardType detectedCompany = DetectCardType(Content);
            Company = detectedCompany;

            switch (detectedCompany)
            {
                case CardType.Unknown:
                    Image = Resources.credit_card;
                    break;

                case CardType.Visa:
                    Image = Resources.visa;
                    break;

                case CardType.MasterCard:
                    Image = Resources.mastercard;
                    break;

                case CardType.AmericanExpress:
                    Image = Resources.americanexpress;
                    break;

                case CardType.Discover:
                    Image = Resources.discover;
                    break;

                case CardType.JCB:
                    Image = Resources.jcb;
                    break;

                case CardType.Maestro:
                    Image = Resources.maestro;
                    break;

                case CardType.DinersClub:
                    Image = Resources.dinersclub;
                    break;

                case CardType.UnionPay:
                    Image = Resources.unionpay;
                    break;

                case CardType.RuPay:
                    Image = Resources.rupay;
                    break;
            }
        }

        [Category("CuoreUI")]
        public bool IsCardValid => privateIsCardValid && DetectCardType(actualText) != CardType.Unknown;

        private bool privateIsCardValid = false;

        private bool IsCardValidGetter
        {
            get
            {
                string cleaned = actualText.Replace(" ", "");
                bool newCardValidValue = cleaned.Length > 11 && cleaned.Length < 20;
                if (privateIsCardValid != newCardValidValue)
                {
                    privateIsCardValid = newCardValidValue;
                    IsCardValidChanged?.Invoke(this, null);
                    return privateIsCardValid;
                }

                privateIsCardValid = newCardValidValue;
                return privateIsCardValid;
            }
        }

        public CardType DetectCardType(string cardNumber)
        {
            cardNumber = cardNumber.Replace(" ", "");

            if (string.IsNullOrWhiteSpace(cardNumber) || !IsCardValidGetter)
                return CardType.Unknown;

            int prefix2 = int.Parse(cardNumber.Substring(0, 2));
            int prefix3 = int.Parse(cardNumber.Substring(0, 3));
            int prefix4 = int.Parse(cardNumber.Substring(0, 4));
            int prefix6 = int.Parse(cardNumber.Substring(0, 6));

            // Visa / Visa Electron
            if (cardNumber.StartsWith("4"))
            {
                return CardType.Visa;
            }

            // MasterCard (incl. 2221–2720)
            if ((prefix2 >= 51 && prefix2 <= 55)
             || (prefix4 >= 2221 && prefix4 <= 2720))
                return CardType.MasterCard;

            // AmEx
            if (cardNumber.StartsWith("34")
             || cardNumber.StartsWith("37"))
                return CardType.AmericanExpress;

            // Discover
            if (cardNumber.StartsWith("6011")
             || cardNumber.StartsWith("65")
             || (prefix6 >= 622126 && prefix6 <= 622925)
             || (prefix3 >= 644 && prefix3 <= 649))
                return CardType.Discover;

            // JCB
            if (prefix4 >= 3528 && prefix4 <= 3589)
                return CardType.JCB;

            // Maestro
            if (prefix4 == 5018
             || prefix4 == 5020
             || prefix4 == 5038
             || prefix4 == 5893
             || prefix4 == 6304
             || prefix4 == 6759
             || prefix4 == 6761
             || prefix4 == 6762
             || prefix4 == 6763)
                return CardType.Maestro;

            // Diners Club
            if (prefix2 == 36 || prefix2 == 38 || prefix2 == 39)
                return CardType.DinersClub;

            // UnionPay
            if (cardNumber.StartsWith("62"))
                return CardType.UnionPay;

            // RuPay
            if ((prefix6 >= 508500 && prefix6 <= 508999)
             || (prefix6 >= 606985 && prefix6 <= 607984)
             || (prefix6 >= 608001 && prefix6 <= 608500)
             || (prefix6 >= 652150 && prefix6 <= 653149))
                return CardType.RuPay;

            // UATP
            if (cardNumber.StartsWith("1"))
                return CardType.UATP;

            return CardType.Unknown;
        }

        public enum CardType
        {
            Unknown,
            Visa,
            MasterCard,
            AmericanExpress,
            Discover,
            JCB,
            Maestro,
            DinersClub,
            UnionPay,
            RuPay,
            UATP
        }

        private CardType privateCompany = CardType.Unknown;
        [Category("CuoreUI")]
        public CardType Company 
        {
            get
            {
                return privateCompany;
            }
            private set
            {
                privateCompany = value;
            }
        }
    }
}
