using System;
using System.Runtime.InteropServices;
using System.Security;

namespace RitmaRestApi.DataSource
{
    public static class SecureStringPassword
    {
        public static SecureString GetPasswordCheckReenter(Func<string, bool> passwordStrengthChecker)
        {
            SecureString pass1, pass2;

            uint trial = 0;
            do
            {
                if (trial > 0) Console.WriteLine($"{Environment.NewLine}Passwords didn't match");
                trial++;
                Console.WriteLine($"{Environment.NewLine}Please insert password:");
                pass1 = SecureStringPassword.GetConsoleSecurePassword(ss => SecureStringPassword.PasswordIsStrongEnoughDefault(ss, passwordStrengthChecker));
                Console.WriteLine($"{Environment.NewLine}Please insert password again:");
                pass2 = SecureStringPassword.GetConsoleSecurePassword(ss => SecureStringPassword.PasswordIsStrongEnoughDefault(ss, passwordStrengthChecker));
            } while (!SecureStringPassword.SecureStringEqual(pass1, pass2));
            return pass1;
        }

        public static string ToBasicString(this SecureString ss)
        {
            IntPtr ssBstr1Ptr = IntPtr.Zero;

            try
            {
                ssBstr1Ptr = Marshal.SecureStringToBSTR(ss);

                string str1 = Marshal.PtrToStringBSTR(ssBstr1Ptr);
                return str1;
            }
            finally
            {
                if (ssBstr1Ptr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(ssBstr1Ptr);
            }
        }
        static bool PasswordIsStrongEnoughDefault(SecureString ss, Func<string, bool> passwordStrengthChecker)
        {
            IntPtr ss_bstr1_ptr = IntPtr.Zero;

            try
            {
                ss_bstr1_ptr = Marshal.SecureStringToBSTR(ss);

                String str1 = Marshal.PtrToStringBSTR(ss_bstr1_ptr);
                return passwordStrengthChecker(str1);
            }
            finally
            {
                if (ss_bstr1_ptr != IntPtr.Zero)
                    Marshal.ZeroFreeBSTR(ss_bstr1_ptr);
            }
        }
        static SecureString GetConsoleSecurePassword(Func<SecureString, bool> passwordStrengthChecker)
        {
            SecureString pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    if (passwordStrengthChecker(pwd))
                        return pwd;
                    Console.WriteLine($"{Environment.NewLine}Password is not strong enough");
                    pwd = new SecureString();
                    continue;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length == 0) continue;
                    pwd.RemoveAt(pwd.Length - 1);
                    Console.Write("\b \b");
                }
                else
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
        }

        static bool SecureStringEqual(SecureString secureString1, SecureString secureString2)
        {
            if (secureString1 == null)
            {
                throw new ArgumentNullException(nameof(secureString1));
            }
            if (secureString2 == null)
            {
                throw new ArgumentNullException(nameof(secureString2));
            }

            if (secureString1.Length != secureString2.Length)
            {
                return false;
            }

            IntPtr ss_bstr1_ptr = IntPtr.Zero;
            IntPtr ss_bstr2_ptr = IntPtr.Zero;

            try
            {
                ss_bstr1_ptr = Marshal.SecureStringToBSTR(secureString1);
                ss_bstr2_ptr = Marshal.SecureStringToBSTR(secureString2);

                String str1 = Marshal.PtrToStringBSTR(ss_bstr1_ptr);
                String str2 = Marshal.PtrToStringBSTR(ss_bstr2_ptr);

                return str1.Equals(str2);
            }
            finally
            {
                if (ss_bstr1_ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ss_bstr1_ptr);
                }

                if (ss_bstr2_ptr != IntPtr.Zero)
                {
                    Marshal.ZeroFreeBSTR(ss_bstr2_ptr);
                }
            }
        }

    }
}