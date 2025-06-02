using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class TCNoValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// İlk 9 haneden, tek hanelerin toplamı (1,3,5,7,9. haneler) * 7'den çift hanelerin toplamı (2,4,6,8. haneler) çıkarılarak 10 ile mod alınır, bu sonuç 10. haneyi vermelidir.

        //İlk 10 hanenin toplamı 10 ile mod alındığında sonuç 11. haneyi vermelidir.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var tcNo = value as string;

            if (string.IsNullOrEmpty(tcNo))
                return ValidationResult.Success;

            if (tcNo.Length != 11 || !long.TryParse(tcNo, out _))
                return new ValidationResult(GetErrorMessage());

            if (tcNo[0] == '0')
                return new ValidationResult(GetErrorMessage());

            // İlk 9 haneden hesaplamalar için
            int sumOdd = 0;  // 1,3,5,7,9. haneler
            int sumEven = 0; // 2,4,6,8. haneler

            for (int i = 0; i < 9; i++)
            {
                int digit = tcNo[i] - '0';
                if (i % 2 == 0) // index 0,2,4,6,8 -> 1.,3.,5.,7.,9. haneler
                    sumOdd += digit;
                else
                    sumEven += digit;
            }

            int digit10 = tcNo[9] - '0';
            int digit11 = tcNo[10] - '0';

            // 10. hane hesaplama: ((tek hane toplamı * 7) - çift hane toplamı) mod 10
            int calcDigit10 = ((sumOdd * 7) - sumEven) % 10;

            // 11. hane hesaplama: İlk 10 hanenin toplamı mod 10
            int sumFirst10 = 0;
            for (int i = 0; i < 10; i++)
            {
                sumFirst10 += tcNo[i] - '0';
            }
            int calcDigit11 = sumFirst10 % 10;

            if (calcDigit10 != digit10 || calcDigit11 != digit11)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return Lang.Value("Invalid Turkish ID Number.");
        }
    }
}
