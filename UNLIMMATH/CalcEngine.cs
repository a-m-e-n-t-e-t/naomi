using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UNLIMMATH
{
    // =====================================================================
    // Инженерный калькулятор с табличной арифметикой.
    //
    // Все числа хранятся как строки — это даёт произвольную точность
    // без использования decimal / double / BigInteger.
    //
    // Базовые операции (+, -, *) выполняются через lookup-таблицы 11×11
    // (Sums / Subs / Mults). Деление и тригонометрия используют
    // итеративные алгоритмы (столбик, ряды Тейлора, метод Ньютона)
    // поверх этих таблиц.
    //
    // Поддерживаются:
    //   • Арифметика: + - * / ^ mod
    //   • Тригонометрия: sin cos tan asin acos atan atan2
    //   • Гиперболические: sinh cosh tanh
    //   • Логарифмы: ln log log2 exp
    //   • Степени/корни: pow sqrt root
    //   • Округление: floor ceil trunc round
    //   • Прочее: abs sign min max hypot
    //   • Комбинаторика: fact ncr npr combrep permrep
    //   • Теория чисел: isprime prime factorize fib euler sigma gcd
    //   • Численные методы: integrate (Симпсон) solve (Ньютон)
    //   • Статистика: mean stddev median
    //   • Переменные: x = 5, y = x + 3
    //   • Константы: pi e phi tau ln2 ln10 c g h na r k
    // =====================================================================
    internal static class CalcEngine
    {
        // -----------------------------------------------------------------
        // Таблицы базовой арифметики.
        // -----------------------------------------------------------------
        private static readonly string[] Numbers =
            { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        private static readonly string[,] Sums = {
            {"+","0","1","2","3","4","5","6","7","8","9"},
            {"0","0","1","2","3","4","5","6","7","8","9"},
            {"1","1","2","3","4","5","6","7","8","9","10"},
            {"2","2","3","4","5","6","7","8","9","10","11"},
            {"3","3","4","5","6","7","8","9","10","11","12"},
            {"4","4","5","6","7","8","9","10","11","12","13"},
            {"5","5","6","7","8","9","10","11","12","13","14"},
            {"6","6","7","8","9","10","11","12","13","14","15"},
            {"7","7","8","9","10","11","12","13","14","15","16"},
            {"8","8","9","10","11","12","13","14","15","16","17"},
            {"9","9","10","11","12","13","14","15","16","17","18"},
        };

        private static readonly string[,] Subs = {
            {"-","0","1","2","3","4","5","6","7","8","9"},
            {"0","0","","","","","","","","",""},
            {"1","1","0","","","","","","","",""},
            {"2","2","1","0","","","","","","",""},
            {"3","3","2","1","0","","","","","",""},
            {"4","4","3","2","1","0","","","","",""},
            {"5","5","4","3","2","1","0","","","",""},
            {"6","6","5","4","3","2","1","0","","",""},
            {"7","7","6","5","4","3","2","1","0","",""},
            {"8","8","7","6","5","4","3","2","1","0",""},
            {"9","9","8","7","6","5","4","3","2","1","0"},
            {"10","10","9","8","7","6","5","4","3","2","1"},
        };

        private static readonly string[,] Mults = {
            {"*","0","1","2","3","4","5","6","7","8","9"},
            {"0","0","0","0","0","0","0","0","0","0","0"},
            {"1","0","1","2","3","4","5","6","7","8","9"},
            {"2","0","2","4","6","8","10","12","14","16","18"},
            {"3","0","3","6","9","12","15","18","21","24","27"},
            {"4","0","4","8","12","16","20","24","28","32","36"},
            {"5","0","5","10","15","20","25","30","35","40","45"},
            {"6","0","6","12","18","24","30","36","42","48","54"},
            {"7","0","7","14","21","28","35","42","49","56","63"},
            {"8","0","8","16","24","32","40","48","56","64","72"},
            {"9","0","9","18","27","36","45","54","63","72","81"},
        };

        // -----------------------------------------------------------------
        // Константы.
        // -----------------------------------------------------------------
        public const string PI   = "3.14159265358979323846";
        public const string E    = "2.71828182845904523536";
        public const string LN10 = "2.30258509299404568401";
        public const string LN2  = "0.69314718055994530941";
        public const string PHI  = "1.61803398874989484820";  // золотое сечение
        public const string TAU  = "6.28318530717958647692";  // 2π

        /// <summary>Количество знаков после запятой в результате деления и тригонометрии.</summary>
        public static int PREC = 20;

        // -----------------------------------------------------------------
        // Кэш умножения: (a, b) → результат. Ключи нормализуются (a ≤ b).
        // -----------------------------------------------------------------
        private static readonly Dictionary<(string, string), string> _multCache =
            new Dictionary<(string, string), string>();

        // -----------------------------------------------------------------
        // Внутреннее исключение для ошибок вычислителя.
        // Evaluate() ловит его и преобразует в строку "ERR: ...".
        // -----------------------------------------------------------------
        private class CalcException : Exception
        {
            public CalcException(string msg) : base(msg) { }
        }

        private struct Result { public string[] res_a; public string[] res_b; }

        // -----------------------------------------------------------------
        // Вспомогательные функции для работы со знаком.
        // -----------------------------------------------------------------
        private static bool IsNeg(string s) { return s.StartsWith("-"); }
        private static string MakeNeg(string s) { return IsNeg(s) ? s.Substring(1) : "-" + s; }
        private static string AbsVal(string s) { return IsNeg(s) ? s.Substring(1) : s; }
        private static bool IsZero(string s) { return Clean(s) == "0"; }

        // -----------------------------------------------------------------
        // Clean — удаление незначащих нулей.
        // -----------------------------------------------------------------
        private static string Clean(string s)
        {
            if (string.IsNullOrEmpty(s)) return "0";
            bool neg = s.StartsWith("-");
            if (neg) s = s.Substring(1);
            if (s.Contains("."))
            {
                string[] p = s.Split('.');
                string ip = p[0].TrimStart('0');
                if (ip == "") ip = "0";
                string fp = p[1].TrimEnd('0');
                s = fp == "" ? ip : ip + "." + fp;
            }
            else
            {
                s = s.TrimStart('0');
                if (s == "") s = "0";
            }
            if (neg && s != "0") s = "-" + s;
            return s;
        }

        // -----------------------------------------------------------------
        // Tbl — поиск в таблице.
        // -----------------------------------------------------------------
        private static string Tbl(string[,] tab, char rowCh, char colCh, int rowCount, int colCount)
        {
            int r = rowCh - '0' + 1;
            int c = colCh - '0' + 1;
            if (r >= 0 && r < rowCount && c >= 0 && c < colCount)
                return tab[r, c];
            return "";
        }

        private static string Tbl(string[,] tab, string row, string col, int rowCount, int colCount)
        {
            if (row.Length == 1 && col.Length == 1)
                return Tbl(tab, row[0], col[0], rowCount, colCount);
            int r = -1, c = -1;
            for (int i = 0; i < rowCount; i++) if (tab[i, 0] == row) { r = i; break; }
            for (int j = 0; j < colCount; j++) if (tab[0, j] == col) { c = j; break; }
            return (r >= 0 && c >= 0) ? tab[r, c] : "";
        }

        // -----------------------------------------------------------------
        // Norm — выравнивание двух чисел по длине.
        // -----------------------------------------------------------------
        private static Result Norm(string[] a, string[] b)
        {
            if (a.Length == 1) a = new string[] { a[0], "" };
            if (b.Length == 1) b = new string[] { b[0], "" };
            while (a[0].Length > b[0].Length) b[0] = "0" + b[0];
            while (a[0].Length < b[0].Length) a[0] = "0" + a[0];
            while (a[1].Length > b[1].Length) b[1] = b[1] + "0";
            while (a[1].Length < b[1].Length) a[1] = a[1] + "0";
            return new Result { res_a = a, res_b = b };
        }

        // -----------------------------------------------------------------
        // Sr — сравнение двух чисел-строк.
        // -----------------------------------------------------------------
        private static string Sr(string a, string b)
        {
            a = Clean(a); b = Clean(b);
            bool na = IsNeg(a), nb = IsNeg(b);
            if (na && !nb) return "<";
            if (!na && nb) return ">";
            if (na && nb)
            {
                string r = Sr(AbsVal(a), AbsVal(b));
                return r == ">" ? "<" : (r == "<" ? ">" : "=");
            }
            string[] ca = a.Split('.'), cb = b.Split('.');
            if (ca[0].Length > cb[0].Length) return ">";
            if (ca[0].Length < cb[0].Length) return "<";
            for (int i = 0; i < ca[0].Length; i++)
            {
                int ia = Array.IndexOf(Numbers, ca[0][i].ToString());
                int ib = Array.IndexOf(Numbers, cb[0][i].ToString());
                if (ia > ib) return ">";
                if (ia < ib) return "<";
            }
            string fa = ca.Length > 1 ? ca[1] : "";
            string fb = cb.Length > 1 ? cb[1] : "";
            int ml = fa.Length > fb.Length ? fa.Length : fb.Length;
            while (fa.Length < ml) fa += "0";
            while (fb.Length < ml) fb += "0";
            for (int i = 0; i < ml; i++)
            {
                int ia = Array.IndexOf(Numbers, fa[i].ToString());
                int ib = Array.IndexOf(Numbers, fb[i].ToString());
                if (ia > ib) return ">";
                if (ia < ib) return "<";
            }
            return "=";
        }

        // -----------------------------------------------------------------
        // AddCarry — прибавление переноса к произведению.
        // -----------------------------------------------------------------
        private static string AddCarry(string product, string carry)
        {
            if (carry == "" || carry == "0") return product;
            if (product.Length == 1)
                return Tbl(Sums, product[0], carry[0], 11, 11);
            string addLast = Tbl(Sums, product[1], carry[0], 11, 11);
            if (addLast.Length == 1)
                return product[0].ToString() + addLast;
            string newFirst = Tbl(Sums, product[0], addLast[0], 11, 11);
            return newFirst + addLast[1].ToString();
        }

        // -----------------------------------------------------------------
        // Sum — сложение.
        // -----------------------------------------------------------------
        private static string Sum(string a, string b)
        {
            a = Clean(a); b = Clean(b);
            bool na = IsNeg(a), nb = IsNeg(b);
            if (na && !nb) return Sub(b, AbsVal(a));
            if (!na && nb) return Sub(a, AbsVal(b));
            if (na && nb) return MakeNeg(Sum(AbsVal(a), AbsVal(b)));
            string[] ca = a.Split('.'), cb = b.Split('.');
            Result t = Norm(ca, cb);
            ca = t.res_a; cb = t.res_b;
            int point = ca[1].Length > 0 ? ca[1].Length : -1;
            string aC = ca[0] + ca[1], bC = cb[0] + cb[1];
            string result = "", perenos = "";
            for (int i = aC.Length - 1; i >= 0; i--)
            {
                string tr = Tbl(Sums, aC[i], bC[i], 11, 11);
                if (perenos != "")
                {
                    if (tr.Length == 1)
                        tr = Tbl(Sums, tr[0], '1', 11, 11);
                    else
                        tr = tr[0].ToString() + Tbl(Sums, tr[1], '1', 11, 11);
                }
                perenos = tr.Length > 1 ? tr.Substring(0, tr.Length - 1) : "";
                result = tr[tr.Length - 1] + result;
            }
            if (perenos != "") result = perenos + result;
            if (point > -1) result = result.Insert(result.Length - point, ".");
            return Clean(result);
        }

        // -----------------------------------------------------------------
        // Sub — вычитание.
        // -----------------------------------------------------------------
        private static string Sub(string a, string b)
        {
            a = Clean(a); b = Clean(b);
            bool na = IsNeg(a), nb = IsNeg(b);
            if (na && !nb) return MakeNeg(Sum(AbsVal(a), AbsVal(b)));
            if (!na && nb) return Sum(a, AbsVal(b));
            if (na && nb) return Sub(AbsVal(b), AbsVal(a));
            string cmp = Sr(a, b);
            if (cmp == "=") return "0";
            if (cmp == "<") return MakeNeg(Sub(b, a));
            string[] ca = a.Split('.'), cb = b.Split('.');
            Result t = Norm(ca, cb);
            ca = t.res_a; cb = t.res_b;
            int point = ca[1].Length > 0 ? ca[1].Length : -1;
            string aC = ca[0] + ca[1], bC = cb[0] + cb[1];
            string result = "", perenos = "";
            for (int i = aC.Length - 1; i >= 0; i--)
            {
                char ai = aC[i], bi = bC[i];
                if (perenos != "")
                {
                    string aiMinus1 = Tbl(Subs, ai.ToString(), "1", 12, 11);
                    if (aiMinus1 == "") { ai = '9'; perenos = "1"; }
                    else { ai = aiMinus1[0]; perenos = ""; }
                }
                string s = "";
                if (Sr(ai.ToString(), bi.ToString()) == "<")
                {
                    s = ai.ToString();
                    ai = '1';
                    perenos = "1";
                }
                string aKey = (ai == '1' && s != "") ? "10" : ai.ToString();
                string tr = Tbl(Subs, aKey, bi.ToString(), 12, 11);
                if (s != "")
                    tr = Tbl(Sums, tr[0], s[0], 11, 11);
                result = (tr.Length > 0 ? tr[tr.Length - 1].ToString() : "0") + result;
            }
            if (point > -1) result = result.Insert(result.Length - point, ".");
            return Clean(result);
        }

        // -----------------------------------------------------------------
        // ShiftRight — сдвиг запятой вправо (×10^n).
        // -----------------------------------------------------------------
        private static string ShiftRight(string s, int n)
        {
            if (n <= 0) return s;
            if (!s.Contains(".")) return s + new string('0', n);
            string[] p = s.Split('.');
            string ip = p[0], fp = p[1];
            if (n >= fp.Length) return ip + fp + new string('0', n - fp.Length);
            return ip + fp.Substring(0, n) + "." + fp.Substring(n);
        }

        // -----------------------------------------------------------------
        // MultBy11 — быстрое умножение на 11.
        // -----------------------------------------------------------------
        private static string MultBy11(string num)
        {
            if (num.Contains(".")) return null;
            string result = num[num.Length - 1].ToString();
            int carry = 0;
            for (int i = num.Length - 2; i >= 0; i--)
            {
                int sum = int.Parse(num[i].ToString()) + int.Parse(num[i + 1].ToString()) + carry;
                carry = sum / 10;
                result = (sum % 10).ToString() + result;
            }
            int first = int.Parse(num[0].ToString()) + carry;
            return first.ToString() + result;
        }

        // -----------------------------------------------------------------
        // Mult — умножение (с трюками ×5, ×25, ×11, ×99...9).
        // -----------------------------------------------------------------
        public static string Mult(string a, string b)
        {
            a = Clean(a); b = Clean(b);
            (string, string) key = (Sr(a, b) == "<") ? (a, b) : (b, a);
            if (_multCache.TryGetValue(key, out string cached))
                return cached;
            bool na = IsNeg(a), nb = IsNeg(b);
            string sign = (na && !nb) || (!na && nb) ? "-" : "";
            a = AbsVal(a); b = AbsVal(b);
            if (IsZero(a) || IsZero(b)) return "0";
            if (b == "1") return sign == "-" ? sign + a : a;
            if (a == "1") return sign == "-" ? sign + b : b;
            if (a == "5" || b == "5")
            {
                string other = (a == "5") ? b : a;
                string r = Div(ShiftRight(other, 1), "2");
                return sign == "-" && r != "0" ? sign + r : r;
            }
            if (a == "25" || b == "25")
            {
                string other = (a == "25") ? b : a;
                string r = Div(ShiftRight(other, 2), "4");
                return sign == "-" && r != "0" ? sign + r : r;
            }
            if (a == "11" || b == "11")
            {
                string other = (a == "11") ? b : a;
                if (!other.Contains("."))
                {
                    string r = MultBy11(other);
                    return sign == "-" && r != "0" ? sign + r : r;
                }
            }
            int zerosA = 0, zerosB = 0;
            while (a.EndsWith("0")) { a = a.Substring(0, a.Length - 1); zerosA++; }
            while (b.EndsWith("0")) { b = b.Substring(0, b.Length - 1); zerosB++; }
            bool bAll9 = true;
            foreach (char c in b) if (c != '9') { bAll9 = false; break; }
            if (bAll9 && b.Length > 1)
            {
                string tenK = ShiftRight(a, b.Length);
                string r = Sub(tenK, a);
                r = ShiftRight(r, zerosA + zerosB);
                return Clean(sign == "-" && r != "0" ? sign + r : r);
            }
            bool aAll9 = true;
            foreach (char c in a) if (c != '9') { aAll9 = false; break; }
            if (aAll9 && a.Length > 1)
            {
                string tenK = ShiftRight(b, a.Length);
                string r = Sub(tenK, b);
                r = ShiftRight(r, zerosA + zerosB);
                return Clean(sign == "-" && r != "0" ? sign + r : r);
            }
            string[] ca = a.Split('.'), cb = b.Split('.');
            int decA = ca.Length > 1 ? ca[1].Length : 0;
            int decB = cb.Length > 1 ? cb[1].Length : 0;
            string aC = (ca[0] + (ca.Length > 1 ? ca[1] : "")).TrimStart('0');
            string bC = (cb[0] + (cb.Length > 1 ? cb[1] : "")).TrimStart('0');
            if (aC == "") aC = "0";
            if (bC == "") bC = "0";
            if (aC == "1" || bC == "1")
            {
                string mantissa = aC == "1" ? bC : aC;
                int tp = decA + decB - zerosA - zerosB;
                string r = mantissa;
                if (tp > 0)
                {
                    if (r.Length <= tp) r = new string('0', tp - r.Length + 1) + r;
                    r = r.Insert(r.Length - tp, ".");
                }
                else if (tp < 0) r = r + new string('0', -tp);
                r = Clean(r);
                return sign == "-" && r != "0" ? sign + r : r;
            }
            var lsum = new List<string>();
            for (int ia = aC.Length - 1; ia >= 0; ia--)
            {
                string ts = "", perenos = "";
                for (int ib = bC.Length - 1; ib >= 0; ib--)
                {
                    string ti = Tbl(Mults, aC[ia], bC[ib], 11, 11);
                    ti = AddCarry(ti, perenos);
                    perenos = ti.Length > 1 ? ti.Substring(0, ti.Length - 1) : "";
                    ts = ti[ti.Length - 1] + ts;
                }
                if (perenos != "") ts = perenos + ts;
                int zc = aC.Length - 1 - ia + zerosA + zerosB;
                if (zc > 0) ts = ts + new string('0', zc);
                lsum.Add(ts);
            }
            string result = "0";
            foreach (string p in lsum) result = Sum(result, p);
            int tp2 = decA + decB;
            if (tp2 > 0)
            {
                if (result.Length <= tp2)
                    result = new string('0', tp2 - result.Length + 1) + result;
                result = result.Insert(result.Length - tp2, ".");
            }
            result = Clean(result);
            string finalResult = sign == "-" && result != "0" ? sign + result : result;
            _multCache[key] = finalResult;
            return finalResult;
        }

        // -----------------------------------------------------------------
        // EstimateDigit — грубая оценка цифры частного.
        // -----------------------------------------------------------------
        private static string EstimateDigit(string remainder, string divisor)
        {
            int rTake = remainder.Length >= 2 ? 2 : 1;
            int dTake = divisor.Length >= 2 ? 2 : 1;
            int rHead = int.Parse(remainder.Substring(0, rTake));
            int dHead = int.Parse(divisor.Substring(0, dTake));
            if (rTake > dTake) dHead *= 10;
            if (dTake > rTake) rHead *= 10;
            int guess = rHead / dHead;
            if (guess > 9) guess = 9;
            if (guess < 1) guess = 1;
            return guess.ToString();
        }

        // -----------------------------------------------------------------
        // Div — деление столбиком.
        // -----------------------------------------------------------------
        public static string Div(string a, string b)
        {
            a = Clean(a); b = Clean(b);
            if (IsZero(b)) throw new CalcException("деление на ноль");
            bool na = IsNeg(a), nb = IsNeg(b);
            string sign = (na && !nb) || (!na && nb) ? "-" : "";
            a = AbsVal(a); b = AbsVal(b);
            if (IsZero(a)) return "0";
            string[] ca = a.Split('.'), cb = b.Split('.');
            int decA = ca.Length > 1 ? ca[1].Length : 0;
            int decB = cb.Length > 1 ? cb[1].Length : 0;
            string aI = (ca[0] + (ca.Length > 1 ? ca[1] : "")).TrimStart('0');
            string bI = (cb[0] + (cb.Length > 1 ? cb[1] : "")).TrimStart('0');
            if (aI == "") aI = "0"; if (bI == "") bI = "0";
            int shift = decA - decB;
            if (shift < 0) { aI = aI + new string('0', -shift); shift = 0; }
            aI = aI + new string('0', PREC + 2);
            string quotient = "", remainder = "0";
            if (bI.Length == 1)
            {
                int bDigit = int.Parse(bI);
                for (int i = 0; i < aI.Length; i++)
                {
                    remainder = remainder.TrimStart('0');
                    remainder = remainder == "" ? aI[i].ToString() : remainder + aI[i].ToString();
                    int remInt = int.Parse(remainder);
                    int q = remInt / bDigit;
                    int newRem = remInt - q * bDigit;
                    quotient += q.ToString();
                    remainder = newRem.ToString();
                }
            }
            else
            {
                int i = 0;
                while (i < aI.Length)
                {
                    remainder = remainder.TrimStart('0');
                    remainder = remainder == "" ? aI[i].ToString() : remainder + aI[i].ToString();
                    if (Sr(remainder, bI) == "<")
                    {
                        int zeros = 0;
                        int j = i + 1;
                        while (Sr(remainder, bI) == "<" && j < aI.Length)
                        {
                            zeros++;
                            remainder = remainder.TrimStart('0');
                            remainder = remainder == "" ? aI[j].ToString() : remainder + aI[j].ToString();
                            j++;
                        }
                        if (Sr(remainder, bI) == "<")
                        {
                            quotient += new string('0', zeros + 1);
                            i = j;
                            continue;
                        }
                        quotient += new string('0', zeros);
                        i = j;
                        string guess = EstimateDigit(remainder, bI);
                        int guessInt = int.Parse(guess);
                        int bestD = 0;
                        for (int d = guessInt; d <= 9; d++)
                        {
                            string prod = Mult(bI, d.ToString());
                            if (Sr(prod, remainder) == ">") break;
                            bestD = d;
                        }
                        if (bestD == 0)
                        {
                            for (int d = guessInt - 1; d >= 1; d--)
                            {
                                string prod = Mult(bI, d.ToString());
                                string cmp = Sr(prod, remainder);
                                if (cmp == "=" || cmp == "<") { bestD = d; break; }
                            }
                        }
                        if (bestD > 0)
                        {
                            quotient += bestD.ToString();
                            remainder = Sub(remainder, Mult(bI, bestD.ToString()));
                        }
                        else quotient += "0";
                        continue;
                    }
                    string guessN = EstimateDigit(remainder, bI);
                    int guessIntN = int.Parse(guessN);
                    int bestDN = 0;
                    for (int d = guessIntN; d <= 9; d++)
                    {
                        string prod = Mult(bI, d.ToString());
                        if (Sr(prod, remainder) == ">") break;
                        bestDN = d;
                    }
                    if (bestDN == 0)
                    {
                        for (int d = guessIntN - 1; d >= 1; d--)
                        {
                            string prod = Mult(bI, d.ToString());
                            string cmp = Sr(prod, remainder);
                            if (cmp == "=" || cmp == "<") { bestDN = d; break; }
                        }
                    }
                    if (bestDN > 0)
                    {
                        quotient += bestDN.ToString();
                        remainder = Sub(remainder, Mult(bI, bestDN.ToString()));
                    }
                    else quotient += "0";
                    i++;
                }
            }
            int totalFrac = (PREC + 2) + shift;
            if (totalFrac > 0 && quotient.Length > totalFrac)
            {
                string intPart = quotient.Substring(0, quotient.Length - totalFrac);
                string fracPart = quotient.Substring(quotient.Length - totalFrac);
                quotient = intPart + "." + fracPart;
            }
            else if (totalFrac > 0)
            {
                quotient = "0." + new string('0', totalFrac - quotient.Length) + quotient;
            }
            quotient = RoundTo(quotient, PREC);
            quotient = Clean(quotient);
            return sign == "-" && quotient != "0" ? sign + quotient : quotient;
        }

        // -----------------------------------------------------------------
        // RoundTo — округление.
        // -----------------------------------------------------------------
        private static string RoundTo(string s, int prec)
        {
            s = Clean(s);
            if (!s.Contains(".")) return s;
            string[] p = s.Split('.');
            if (p[1].Length <= prec) return s;
            string kept = p[1].Substring(0, prec);
            string roundDig = p[1].Substring(prec, 1);
            string intPart = p[0] == "" ? "0" : p[0];
            if (Sr(roundDig, "5") == ">" || Sr(roundDig, "5") == "=")
            {
                if (prec == 0) intPart = Sum(intPart, "1");
                else
                {
                    string one = "0." + new string('0', prec - 1) + "1";
                    return Clean(Sum(intPart + "." + kept, one));
                }
            }
            return Clean(intPart + "." + kept);
        }

        // -----------------------------------------------------------------
        // Mod, Gcd, Fact, NCr, NPr.
        // -----------------------------------------------------------------
        public static string Mod(string a, string b)
        {
            if (IsZero(b)) throw new CalcException("mod by zero");
            a = Clean(a); b = Clean(b);
            string q = Div(a, b);
            string[] qa = q.Split('.');
            string intQ = qa[0];
            string prod = Mult(intQ, b);
            return Clean(Sub(a, prod));
        }

        public static string Gcd(string a, string b)
        {
            a = AbsVal(Clean(a)); b = AbsVal(Clean(b));
            while (!IsZero(b))
            {
                string r = Mod(a, b);
                a = b; b = r;
            }
            return Clean(a);
        }

        public static string Fact(string n)
        {
            n = Clean(n);
            if (IsNeg(n)) throw new CalcException("факториал отрицательного");
            if (IsZero(n) || n == "1") return "1";
            string result = "1";
            string i = "1";
            while (Sr(i, n) == "<" || Sr(i, n) == "=")
            {
                result = Mult(result, i);
                i = Sum(i, "1");
            }
            return result;
        }

        public static string NCr(string n, string k)
        {
            if (Sr(k, n) == ">") return "0";
            if (Sr(k, "0") == "=" || Sr(k, n) == "=") return "1";
            string num = Fact(n);
            string den = Mult(Fact(k), Fact(Sub(n, k)));
            return Div(num, den);
        }

        public static string NPr(string n, string k)
        {
            if (Sr(k, n) == ">") return "0";
            string num = Fact(n);
            string den = Fact(Sub(n, k));
            return Div(num, den);
        }

        // -----------------------------------------------------------------
        // PowInt — двоичное возведение в степень.
        // -----------------------------------------------------------------
        public static string PowInt(string x, string n)
        {
            x = Clean(x);
            if (IsZero(n)) return "1";
            if (IsZero(x)) return "0";
            string result = "1";
            string base_ = x;
            string exp = n;
            while (!IsZero(exp))
            {
                string[] ep = exp.Split('.');
                string lastDig = ep[0][ep[0].Length - 1].ToString();
                if (lastDig == "1" || lastDig == "3" || lastDig == "5" || lastDig == "7" || lastDig == "9")
                    result = Mult(result, base_);
                base_ = Mult(base_, base_);
                exp = Div(exp, "2");
                string[] ep2 = exp.Split('.');
                exp = ep2[0];
                if (IsZero(exp)) break;
            }
            return Clean(result);
        }

        // -----------------------------------------------------------------
        // Exp — экспонента.
        // -----------------------------------------------------------------
        public static string Exp(string x)
        {
            x = Clean(x);
            if (IsZero(x)) return "1";
            bool neg = IsNeg(x);
            if (neg) x = AbsVal(x);
            string[] ca = x.Split('.');
            string intPart = ca[0];
            string fracPart = ca.Length > 1 ? ca[1] : "0";
            string expInt = PowInt(E, intPart);
            string frac = "0." + fracPart;
            string term = "1";
            string result = "1";
            string k = "1";
            for (int iter = 0; iter < 60; iter++)
            {
                term = Div(Mult(term, frac), k);
                result = Sum(result, term);
                k = Sum(k, "1");
                if (IsZero(Clean(term)) || Sr(AbsVal(term), "0.000000000000000000001") == "<") break;
            }
            string answer = Mult(expInt, result);
            if (neg) answer = Div("1", answer);
            return Clean(RoundTo(answer, PREC));
        }

        // -----------------------------------------------------------------
        // Ln — натуральный логарифм через artanh.
        // -----------------------------------------------------------------
        public static string Ln(string x)
        {
            x = Clean(x);
            if (IsZero(x)) throw new CalcException("ln от нуля");
            if (IsNeg(x))  throw new CalcException("ln от отрицательного");
            if (x == "1") return "0";
            string k = "0";
            string two = "2";
            while (Sr(x, two) == ">") { x = Div(x, two); k = Sum(k, "1"); }
            while (Sr(x, "0.5") == "<") { x = Mult(x, two); k = Sub(k, "1"); }
            string z = Div(Sub(x, "1"), Sum(x, "1"));
            string result = "0";
            string term = z;
            string kk = "1";
            string z2 = Mult(z, z);
            for (int iter = 0; iter < 100; iter++)
            {
                result = Sum(result, term);
                string nextK = Sum(kk, "2");
                term = Div(Mult(Mult(term, z2), kk), nextK);
                kk = nextK;
                if (IsZero(Clean(term)) || Sr(AbsVal(term), "0.000000000000000000001") == "<") break;
            }
            result = Mult(result, "2");
            result = Sum(result, Mult(k, LN2));
            return Clean(result);
        }

        // -----------------------------------------------------------------
        // Log — логарифм по основанию.
        // -----------------------------------------------------------------
        public static string Log(string x, string base_)
        {
            if (base_ == "10") return Div(Ln(x), LN10);
            if (base_ == "2")  return Div(Ln(x), LN2);
            return Div(Ln(x), Ln(base_));
        }

        public static string Log2(string x) { return Div(Ln(x), LN2); }

        // -----------------------------------------------------------------
        // ReduceAngle — приведение угла к [0, 2π).
        // -----------------------------------------------------------------
        private static string ReduceAngle(string x)
        {
            string twoPi = Mult(PI, "2");
            x = Clean(x);
            while (Sr(x, twoPi) == ">" || Sr(x, twoPi) == "=")
                x = Sub(x, twoPi);
            while (IsNeg(x))
                x = Sum(x, twoPi);
            return x;
        }

        // -----------------------------------------------------------------
        // Sin — синус через ряд Тейлора.
        //   sin(x) = x − x³/3! + x⁵/5! − ...
        //   term_{n+1} = term_n * x² / ((2n+2)(2n+3)), при k = n+1: denom = (2k)(2k+1)
        // -----------------------------------------------------------------
        public static string Sin(string x)
        {
            x = Clean(x);
            bool neg = IsNeg(x);
            if (neg) x = AbsVal(x);
            x = ReduceAngle(x);
            string pi = PI;
            if (Sr(x, pi) == ">" || Sr(x, pi) == "=")
            {
                x = Sub(x, pi);
                neg = !neg;
            }
            string term = x;
            string result = x;
            string k = "1";
            string x2 = Mult(x, x);
            bool subtract = true;
            for (int iter = 0; iter < 100; iter++)
            {
                string denom = Mult(Mult(k, "2"), Sum(Mult(k, "2"), "1"));
                term = Div(Mult(term, x2), denom);
                if (subtract) result = Sub(result, term);
                else result = Sum(result, term);
                subtract = !subtract;
                k = Sum(k, "1");
                if (IsZero(Clean(term)) || Sr(AbsVal(term), "0.000000000000000000001") == "<") break;
            }
            if (neg) result = MakeNeg(result);
            return Clean(RoundTo(result, PREC));
        }

        // -----------------------------------------------------------------
        // Cos — косинус через ряд Тейлора.
        //   cos(x) = 1 − x²/2! + x⁴/4! − ...
        //   term_{n+1} = term_n * x² / ((2n+1)(2n+2)), при k = n+1: denom = (2k-1)(2k)
        // -----------------------------------------------------------------
        public static string Cos(string x)
        {
            x = Clean(x);
            x = AbsVal(x);
            x = ReduceAngle(x);
            bool neg = false;
            string pi = PI;
            if (Sr(x, pi) == ">" || Sr(x, pi) == "=")
            {
                x = Sub(x, pi);
                neg = true;
            }
            string term = "1";
            string result = "1";
            string k = "1";
            string x2 = Mult(x, x);
            bool subtract = true;
            for (int iter = 0; iter < 100; iter++)
            {
                string denom = Mult(Sub(Mult(k, "2"), "1"), Mult(k, "2"));
                term = Div(Mult(term, x2), denom);
                if (subtract) result = Sub(result, term);
                else result = Sum(result, term);
                subtract = !subtract;
                k = Sum(k, "1");
                if (IsZero(Clean(term)) || Sr(AbsVal(term), "0.000000000000000000001") == "<") break;
            }
            if (neg) result = MakeNeg(result);
            return Clean(RoundTo(result, PREC));
        }

        // -----------------------------------------------------------------
        // Tan — тангенс.
        // -----------------------------------------------------------------
        public static string Tan(string x)
        {
            string c = Cos(x);
            if (IsZero(c)) throw new CalcException("tan не определён");
            return Clean(Div(Sin(x), c));
        }

        // -----------------------------------------------------------------
        // Asin — арксинус.
        //   asin(x) = sum C(2n,n)/4^n * x^(2n+1)/(2n+1)
        //   term_{n+1} = term_n * x² * (2n+1)² / (2(n+1)(2n+3))
        // -----------------------------------------------------------------
        public static string Asin(string x)
        {
            x = Clean(x);
            if (Sr(AbsVal(x), "1") == ">") throw new CalcException("asin: аргумент вне [−1, 1]");
            if (x == "1")  return Div(PI, "2");
            if (x == "-1") return MakeNeg(Div(PI, "2"));
            string term = x;
            string result = x;
            string x2 = Mult(x, x);
            int n = 0;
            for (int iter = 0; iter < 300; iter++)
            {
                string num = Mult((2 * n + 1).ToString(), (2 * n + 1).ToString());
                string den = Mult((2 * (n + 1)).ToString(), (2 * n + 3).ToString());
                term = Div(Mult(Mult(term, x2), num), den);
                result = Sum(result, term);
                n++;
                if (IsZero(Clean(term)) || Sr(AbsVal(term), "0.0000000000000000000001") == "<") break;
            }
            return Clean(RoundTo(result, PREC));
        }

        // -----------------------------------------------------------------
        // Acos — арккосинус = π/2 − asin(x).
        // -----------------------------------------------------------------
        public static string Acos(string x)
        {
            x = Clean(x);
            if (Sr(AbsVal(x), "1") == ">") throw new CalcException("acos: аргумент вне [−1, 1]");
            string a = Asin(x);
            return Clean(RoundTo(Sub(Div(PI, "2"), a), PREC));
        }

        // -----------------------------------------------------------------
        // Atan — арктангенс = asin(x / sqrt(1 + x²)).
        //   Это надёжнее ряда Тейлора atan, который сходится медленно
        //   при |x| → 1.
        // -----------------------------------------------------------------
        public static string Atan(string x)
        {
            x = Clean(x);
            if (IsZero(x)) return "0";
            bool neg = IsNeg(x);
            if (neg) x = AbsVal(x);
            string x2Plus1 = Sum(Mult(x, x), "1");
            string sqrtPart = Sqrt(x2Plus1);
            string arg = Div(x, sqrtPart);
            string result = Asin(arg);
            if (neg) result = MakeNeg(result);
            return Clean(RoundTo(result, PREC));
        }

        // -----------------------------------------------------------------
        // Atan2 — угол точки (x, y).
        // -----------------------------------------------------------------
        public static string Atan2(string y, string x)
        {
            y = Clean(y); x = Clean(x);
            if (IsZero(x))
            {
                if (IsZero(y)) return "0";
                if (IsNeg(y))  return MakeNeg(Div(PI, "2"));
                return Div(PI, "2");
            }
            if (IsZero(y)) return IsNeg(x) ? PI : "0";
            string r = Atan(Div(y, x));
            if (IsNeg(x))
            {
                if (IsNeg(y)) return Clean(Sub(r, PI));
                return Clean(Sum(r, PI));
            }
            return r;
        }

        // -----------------------------------------------------------------
        // Гиперболические функции.
        // -----------------------------------------------------------------
        public static string Sinh(string x)
        {
            x = Clean(x);
            string ex = Exp(x);
            string emx = Div("1", ex);
            return Clean(RoundTo(Div(Sub(ex, emx), "2"), PREC));
        }

        public static string Cosh(string x)
        {
            x = Clean(x);
            string ex = Exp(x);
            string emx = Div("1", ex);
            return Clean(RoundTo(Div(Sum(ex, emx), "2"), PREC));
        }

        public static string Tanh(string x)
        {
            string c = Cosh(x);
            if (IsZero(c)) throw new CalcException("tanh не определён");
            return Clean(Div(Sinh(x), c));
        }

        // -----------------------------------------------------------------
        // Sqrt, Root, Pow.
        // -----------------------------------------------------------------
        public static string Sqrt(string x)
        {
            x = Clean(x);
            if (IsNeg(x)) throw new CalcException("sqrt от отрицательного");
            if (IsZero(x)) return "0";
            if (x == "1") return "1";
            // Улучшенное начальное приближение через log10:
            // sqrt(x) ≈ 10^(floor(log10(x)/2))
            string y = x;
            try
            {
                double xFloat = double.Parse(x, CultureInfo.InvariantCulture);
                if (xFloat > 0)
                {
                    double guess = Math.Pow(10, Math.Floor(Math.Log10(xFloat) / 2));
                    if (guess > 0)
                        y = guess.ToString("0", CultureInfo.InvariantCulture);
                }
            }
            catch { }
            string prev = "0";
            for (int iter = 0; iter < 200; iter++)
            {
                string quot = Div(x, y);
                string next = Div(Sum(y, quot), "2");
                next = Clean(next);
                if (Sr(AbsVal(Sub(next, prev)), "0.000000000000000000001") == "<") break;
                prev = next;
                y = next;
            }
            return Clean(RoundTo(y, PREC));
        }

        public static string Root(string x, string n)
        {
            if (IsZero(x)) return "0";
            if (n == "1") return Clean(x);
            string lnX = Ln(x);
            return Clean(RoundTo(Exp(Div(lnX, n)), PREC));
        }

        public static string Pow(string x, string y)
        {
            x = Clean(x); y = Clean(y);
            if (IsZero(y)) return "1";
            if (IsZero(x)) return "0";
            string[] yp = y.Split('.');
            bool isInt = (yp.Length == 1) || (yp[1].TrimEnd('0') == "");
            if (isInt && !IsNeg(y))
                return Clean(PowInt(x, yp[0].TrimStart('0') == "" ? "0" : yp[0].TrimStart('0')));
            if (isInt && IsNeg(y))
                return Div("1", PowInt(x, AbsVal(yp[0])));
            if (IsNeg(x)) throw new CalcException("дробная степень отрицательного");
            string lnX = Ln(x);
            return Clean(RoundTo(Exp(Mult(y, lnX)), PREC));
        }

        // -----------------------------------------------------------------
        // Округление.
        // -----------------------------------------------------------------
        public static string Floor(string x)
        {
            x = Clean(x);
            if (!x.Contains(".")) return x;
            string[] p = x.Split('.');
            string intPart = p[0] == "" ? "0" : p[0];
            string fp = p[1];
            if (IsNeg(x))
            {
                if (fp.TrimEnd('0') == "")
                    return intPart == "0" ? "0" : "-" + AbsVal(intPart);
                return "-" + Sum(AbsVal(intPart), "1");
            }
            return Clean(intPart);
        }

        public static string Ceil(string x)
        {
            x = Clean(x);
            if (!x.Contains(".")) return x;
            string[] p = x.Split('.');
            string intPart = p[0] == "" ? "0" : p[0];
            string fp = p[1];
            if (IsNeg(x))
            {
                if (intPart == "0") return "0";
                return "-" + AbsVal(intPart);
            }
            if (fp.TrimEnd('0') == "") return Clean(intPart);
            return Clean(Sum(intPart, "1"));
        }

        public static string Trunc(string x)
        {
            x = Clean(x);
            if (!x.Contains(".")) return x;
            string intPart = x.Split('.')[0];
            if (intPart == "") intPart = "0";
            return Clean(intPart);
        }

        public static string RoundN(string x, string n)
        {
            int prec = 0;
            if (!string.IsNullOrEmpty(n) && int.TryParse(n, out int p)) prec = p;
            return RoundTo(x, prec);
        }

        // -----------------------------------------------------------------
        // Прочие функции.
        // -----------------------------------------------------------------
        public static string Sign(string x)
        {
            x = Clean(x);
            if (IsZero(x)) return "0";
            return IsNeg(x) ? "-1" : "1";
        }

        public static string Min(string a, string b) { return Sr(a, b) == "<" ? a : b; }
        public static string Max(string a, string b) { return Sr(a, b) == ">" ? a : b; }
        public static string Hypot(string a, string b) { return Sqrt(Sum(Mult(a, a), Mult(b, b))); }

        // -----------------------------------------------------------------
        // Теория чисел.
        // -----------------------------------------------------------------
        public static string IsPrime(string n)
        {
            n = Clean(n);
            if (IsNeg(n) || IsZero(n)) return "0";
            if (n == "1") return "0";
            if (n == "2") return "1";
            if (int.Parse(n[n.Length - 1].ToString()) % 2 == 0) return "0";
            if (n == "3") return "1";
            if (Mod(n, "3") == "0") return "0";
            string i = "5";
            string sqrtN = Sqrt(n);
            while (Sr(i, sqrtN) == "<" || Sr(i, sqrtN) == "=")
            {
                if (Mod(n, i) == "0") return "0";
                if (Mod(n, Sum(i, "2")) == "0") return "0";
                i = Sum(i, "6");
            }
            return "1";
        }

        public static string PrimeN(string n)
        {
            n = Clean(n);
            if (IsZero(n) || IsNeg(n)) throw new CalcException("индекс простого числа");
            if (n == "1") return "2";
            string count = "1";
            string candidate = "3";
            while (Sr(count, n) == "<")
            {
                if (IsPrime(candidate) == "1")
                {
                    count = Sum(count, "1");
                    if (Sr(count, n) == "=") return candidate;
                }
                candidate = Sum(candidate, "2");
            }
            return candidate;
        }

        public static string Factorize(string n)
        {
            n = AbsVal(Clean(n));
            if (n == "0") return "0";
            if (n == "1") return "1";
            var factors = new List<string>();
            string p = "2";
            string sqrtN = Sqrt(n);
            while ((Sr(p, sqrtN) == "<" || Sr(p, sqrtN) == "=") && n != "1")
            {
                string cnt = "0";
                while (Mod(n, p) == "0")
                {
                    n = Div(n, p);
                    cnt = Sum(cnt, "1");
                }
                if (cnt != "0")
                    factors.Add(cnt == "1" ? p : p + "^" + cnt);
                p = Sum(p, "1");
            }
            if (n != "1")
                factors.Add(n);
            return string.Join(" * ", factors);
        }

        public static string Fib(string n)
        {
            n = Clean(n);
            if (IsNeg(n)) throw new CalcException("fib от отрицательного");
            if (IsZero(n)) return "0";
            if (n == "1" || n == "2") return "1";
            string a = "0", b = "1";
            int nInt = int.Parse(n);
            for (int i = 0; i < nInt; i++)
            {
                string newB = Sum(a, b);
                a = b;
                b = newB;
            }
            return a;
        }

        public static string EulerPhi(string n)
        {
            n = AbsVal(Clean(n));
            if (n == "0") return "0";
            if (n == "1") return "1";
            string result = n;
            string p = "2";
            string sqrtN = Sqrt(n);
            while (Sr(p, sqrtN) == "<" || Sr(p, sqrtN) == "=")
            {
                if (Mod(n, p) == "0")
                {
                    while (Mod(n, p) == "0")
                        n = Div(n, p);
                    result = Div(Mult(result, Sub(p, "1")), p);
                }
                p = Sum(p, "1");
            }
            if (n != "1")
                result = Div(Mult(result, Sub(n, "1")), n);
            return Clean(result);
        }

        public static string Sigma(string n)
        {
            n = AbsVal(Clean(n));
            if (n == "0") return "0";
            if (n == "1") return "1";
            string result = "0";
            string i = "1";
            string sqrtN = Sqrt(n);
            while (Sr(i, sqrtN) == "<" || Sr(i, sqrtN) == "=")
            {
                if (Mod(n, i) == "0")
                {
                    result = Sum(result, i);
                    string other = Div(n, i);
                    if (Sr(other, i) == ">")
                        result = Sum(result, other);
                }
                i = Sum(i, "1");
            }
            return Clean(result);
        }

        public static string CombRep(string n, string k) { return NCr(Sum(Sub(n, "1"), k), k); }
        public static string PermRep(string n, string k) { return PowInt(n, k); }

        // -----------------------------------------------------------------
        // Статистика.
        // -----------------------------------------------------------------
        public static string Mean(params string[] args)
        {
            var list = new List<string>();
            foreach (string a in args) if (!string.IsNullOrEmpty(a)) list.Add(a);
            if (list.Count == 0) return "0";
            string s = "0";
            foreach (string a in list) s = Sum(s, a);
            return Div(s, list.Count.ToString());
        }

        public static string StdDev(params string[] args)
        {
            var list = new List<string>();
            foreach (string a in args) if (!string.IsNullOrEmpty(a)) list.Add(a);
            if (list.Count < 2) return "0";
            string m = Mean(list.ToArray());
            string s = "0";
            foreach (string a in list)
            {
                string d = Sub(a, m);
                s = Sum(s, Mult(d, d));
            }
            return Sqrt(Div(s, (list.Count - 1).ToString()));
        }

        public static string Median(params string[] args)
        {
            var list = new List<string>();
            foreach (string a in args) if (!string.IsNullOrEmpty(a)) list.Add(a);
            if (list.Count == 0) return "0";
            list.Sort((x, y) => {
                double dx = double.Parse(Clean(x), CultureInfo.InvariantCulture);
                double dy = double.Parse(Clean(y), CultureInfo.InvariantCulture);
                return dx.CompareTo(dy);
            });
            int n = list.Count;
            if (n % 2 == 1) return list[n / 2];
            return Div(Sum(list[n / 2 - 1], list[n / 2]), "2");
        }

        // -----------------------------------------------------------------
        // Численное интегрирование (Симпсон).
        // -----------------------------------------------------------------
        public static string Integrate(string fStr, string aStr, string bStr, string nStr)
        {
            string a = Clean(aStr);
            string b = Clean(bStr);
            int nInt = 1000;
            if (!string.IsNullOrEmpty(nStr) && int.TryParse(nStr, out int np)) nInt = np;
            if (nInt < 2) nInt = 2;
            nInt = nInt / 2 * 2; // чётное
            string h = Div(Sub(b, a), nInt.ToString());
            string s = EvalWithX(fStr, a);
            for (int i = 1; i < nInt; i++)
            {
                string xi = Sum(a, Mult(i.ToString(), h));
                string fi = EvalWithX(fStr, xi);
                if (i % 2 == 1) s = Sum(s, Mult("4", fi));
                else            s = Sum(s, Mult("2", fi));
            }
            s = Sum(s, EvalWithX(fStr, b));
            return Clean(RoundTo(Div(Mult(h, s), "3"), PREC));
        }

        // -----------------------------------------------------------------
        // Решение уравнений методом Ньютона.
        // -----------------------------------------------------------------
        public static string Solve(string fStr, string x0Str, string itersStr)
        {
            string x = Clean(x0Str);
            string h = "0.0000000001";
            int maxIters = 50;
            if (!string.IsNullOrEmpty(itersStr) && int.TryParse(itersStr, out int it)) maxIters = it;
            for (int iter = 0; iter < maxIters; iter++)
            {
                string fx = EvalWithX(fStr, x);
                if (IsZero(fx)) return Clean(x);
                string fxp = EvalWithX(fStr, Sum(x, h));
                string fxm = EvalWithX(fStr, Sub(x, h));
                string dfx = Div(Sub(fxp, fxm), Mult("2", h));
                if (IsZero(dfx)) throw new CalcException("производная равна нулю");
                string xNew = Sub(x, Div(fx, dfx));
                xNew = Clean(xNew);
                if (Sr(AbsVal(Sub(xNew, x)), "0.000000000000000000001") == "<")
                    return Clean(RoundTo(xNew, PREC));
                x = xNew;
            }
            return Clean(RoundTo(x, PREC));
        }

        // =================================================================
        // ПЕРЕМЕННЫЕ И КОНСТАНТЫ
        // =================================================================
        private static readonly Dictionary<string, string> _variables =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "pi", PI }, { "e", E }, { "phi", PHI }, { "tau", TAU },
                { "ln2", LN2 }, { "ln10", LN10 },
                // Физические константы (SI)
                { "c",  "299792458" },                                                       // скорость света, м/с
                { "g",  "9.80665" },                                                          // ускорение своб. падения, м/с²
                { "h",  "0.000000000000000000000000000000000662606957" },                     // постоянная Планка, Дж·с
                { "na", "0.00000000000000000000000602214076" },                               // число Авогадро, 1/моль
                { "r",  "8.31446261815324" },                                                 // газовая постоянная, Дж/(моль·К)
                { "k",  "0.00000000000000000000000000000000001380649" },                      // постоянная Больцмана, Дж/К
            };

        // =================================================================
        // ПАРСЕР
        //   Expr   → Term (('+' | '-') Term)*
        //   Term   → Factor (('*' | '/' | 'mod') Factor)*
        //   Factor → Unary ('^' Factor)?            // ^ — правоассоциативно
        //   Unary  → ('-' | '+')? Primary
        //   Primary→ '(' Expr ')' | число | функция(args) | константа
        // =================================================================

        private static string _expr;
        private static int _pos;

        private static string Peek()
        {
            return _pos < _expr.Length ? _expr.Substring(_pos, 1) : "";
        }

        private static string Peek2()
        {
            return _pos < _expr.Length - 1 ? _expr.Substring(_pos, 2) : "";
        }

        private static void SkipWs()
        {
            while (_pos < _expr.Length && (_expr[_pos] == ' ' || _expr[_pos] == '\t'))
                _pos++;
        }

        private static string ParseExpr()
        {
            SkipWs();
            string left = ParseTerm();
            SkipWs();
            while (Peek() != "" && (Peek() == "+" || Peek() == "-"))
            {
                string op = Peek(); _pos++; SkipWs();
                string right = ParseTerm();
                left = op == "+" ? Sum(left, right) : Sub(left, right);
                SkipWs();
            }
            return left;
        }

        private static string ParseTerm()
        {
            SkipWs();
            string left = ParseFactor();
            SkipWs();
            while (Peek() != "" && (Peek() == "*" || Peek() == "/" || Peek2() == "mo"))
            {
                SkipWs();
                if (Peek() == "*") { _pos++; SkipWs(); left = Mult(left, ParseFactor()); }
                else if (Peek() == "/") { _pos++; SkipWs(); left = Div(left, ParseFactor()); }
                else if (Peek2() == "mo") { _pos += 3; SkipWs(); left = Mod(left, ParseFactor()); }
                SkipWs();
            }
            return left;
        }

        /// <summary>
        /// Factor — возведение в степень. ^ правоассоциативно:
        /// 2^3^2 = 2^(3^2) = 2^9 = 512.
        /// </summary>
        private static string ParseFactor()
        {
            SkipWs();
            string left = ParseUnary();
            SkipWs();
            if (Peek() == "^")
            {
                _pos++; SkipWs();
                string right = ParseFactor(); // рекурсивно — правоассоциативно
                left = Pow(left, right);
            }
            return left;
        }

        private static string ParseUnary()
        {
            SkipWs();
            if (Peek() == "-") { _pos++; return MakeNeg(ParseUnary()); }
            if (Peek() == "+") { _pos++; return ParseUnary(); }
            return ParsePrimary();
        }

        private static string ParsePrimary()
        {
            SkipWs();
            if (Peek() == "(")
            {
                _pos++; SkipWs();
                string r = ParseExpr();
                SkipWs();
                if (Peek() == ")") _pos++;
                return r;
            }
            if ("0123456789.".Contains(Peek()))
            {
                string num = "";
                while (_pos < _expr.Length && "0123456789.".Contains(_expr[_pos].ToString()))
                {
                    num += _expr[_pos];
                    _pos++;
                }
                return num;
            }
            if (_pos < _expr.Length && char.IsLetter(_expr[_pos]))
            {
                string name = "";
                // Имя функции может содержать буквы, цифры и подчёркивание
                // (atan2, log2, is_prime и т.д.), но должно начинаться с буквы.
                while (_pos < _expr.Length && (char.IsLetterOrDigit(_expr[_pos]) || _expr[_pos] == '_'))
                {
                    name += _expr[_pos];
                    _pos++;
                }
                string nameLower = name.ToLower();
                if (_variables.TryGetValue(nameLower, out string constVal) && Peek() != "(")
                    return constVal;
                SkipWs();
                if (Peek() == "(")
                {
                    _pos++; SkipWs();

                    // Спец-функции: первый аргумент — литеральное выражение
                    // (не вычисляется немедленно). Нужно для solve(f, x0) и
                    // integrate(f, a, b), где f содержит переменную x.
                    if (nameLower == "solve" || nameLower == "integrate")
                    {
                        int start = _pos;
                        int depth = 0;
                        while (_pos < _expr.Length)
                        {
                            char ch = _expr[_pos];
                            if (ch == '(') depth++;
                            else if (ch == ')')
                            {
                                if (depth == 0) break;
                                depth--;
                            }
                            else if (ch == ',' && depth == 0) break;
                            _pos++;
                        }
                        string fStr = _expr.Substring(start, _pos - start).Trim();
                        SkipWs();
                        var args = new List<string> { fStr };
                        while (Peek() == ",")
                        {
                            _pos++; SkipWs();
                            args.Add(ParseExpr());
                            SkipWs();
                        }
                        if (Peek() == ")") _pos++;
                        return CallFunc(nameLower, args);
                    }

                    // Обычные функции: аргументы вычисляются немедленно.
                    var normalArgs = new List<string>();
                    if (Peek() != ")")
                    {
                        normalArgs.Add(ParseExpr());
                        SkipWs();
                        while (Peek() == ",")
                        {
                            _pos++; SkipWs();
                            normalArgs.Add(ParseExpr());
                            SkipWs();
                        }
                    }
                    if (Peek() == ")") _pos++;
                    return CallFunc(nameLower, normalArgs);
                }
                throw new CalcException("неизвестное имя '" + name + "'");
            }
            throw new CalcException("неожиданный символ '" + Peek() + "'");
        }

        private static string CallFunc(string name, List<string> args)
        {
            string a = args.Count > 0 ? args[0] : "";
            string b = args.Count > 1 ? args[1] : "";
            string c = args.Count > 2 ? args[2] : "";
            switch (name)
            {
                case "sin":   return Sin(a);
                case "cos":   return Cos(a);
                case "tan":   return Tan(a);
                case "asin":  return Asin(a);
                case "acos":  return Acos(a);
                case "atan":  return Atan(a);
                case "atan2": return Atan2(a, b);
                case "sinh":  return Sinh(a);
                case "cosh":  return Cosh(a);
                case "tanh":  return Tanh(a);
                case "exp":   return Exp(a);
                case "ln":    return Ln(a);
                case "log":   return string.IsNullOrEmpty(b) ? Log(a, "10") : Log(a, b);
                case "log2":  return Log2(a);
                case "sqrt":  return Sqrt(a);
                case "root":  return Root(a, b);
                case "pow":   return Pow(a, b);
                case "fact":  return Fact(a);
                case "abs":   return AbsVal(a);
                case "mod":   return Mod(a, b);
                case "gcd":   return Gcd(a, b);
                case "ncr":   return NCr(a, b);
                case "npr":   return NPr(a, b);
                case "floor": return Floor(a);
                case "ceil":  return Ceil(a);
                case "trunc": return Trunc(a);
                case "round": return RoundN(a, string.IsNullOrEmpty(b) ? "0" : b);
                case "sign":  return Sign(a);
                case "min":   return string.IsNullOrEmpty(b) ? a : Min(a, b);
                case "max":   return string.IsNullOrEmpty(b) ? a : Max(a, b);
                case "hypot": return Hypot(a, b);
                case "isprime":   return IsPrime(a);
                case "prime":     return PrimeN(a);
                case "factorize": return Factorize(a);
                case "fib":       return Fib(a);
                case "euler":     return EulerPhi(a);
                case "sigma":     return Sigma(a);
                case "combrep":   return CombRep(a, b);
                case "permrep":   return PermRep(a, b);
                case "mean":      return Mean(args.ToArray());
                case "stddev":    return StdDev(args.ToArray());
                case "median":    return Median(args.ToArray());
                case "integrate":
                    if (args.Count < 3) throw new CalcException("integrate требует (f, a, b)");
                    return Integrate(a, b, c, args.Count > 3 ? args[3] : "1000");
                case "solve":
                    if (args.Count < 2) throw new CalcException("solve требует (f, x0)");
                    return Solve(a, b, args.Count > 2 ? args[2] : "50");
                default:
                    throw new CalcException("неизвестная функция '" + name + "'");
            }
        }

        /// <summary>
        /// Вычисляет выражение, подставляя значение переменной x.
        /// Используется в Integrate и Solve.
        /// </summary>
        private static string EvalWithX(string exprStr, string xValue)
        {
            string oldX = null;
            bool hadX = _variables.TryGetValue("x", out oldX);
            _variables["x"] = Clean(xValue);
            try
            {
                _expr = exprStr;
                _pos = 0;
                return Clean(ParseExpr());
            }
            finally
            {
                if (hadX) _variables["x"] = oldX;
                else _variables.Remove("x");
            }
        }

        /// <summary>
        /// Публичная точка входа: вычисляет значение арифметического выражения.
        /// Поддерживает присваивание переменных: «x = 5».
        /// </summary>
        public static string Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return "0";

            // Обработка присваивания: «name = expr»
            int eqIdx = expression.IndexOf('=');
            if (eqIdx > 0 && !expression.Contains("==") && !expression.Contains("<=") && !expression.Contains(">="))
            {
                string name = expression.Substring(0, eqIdx).Trim();
                bool validName = name.Length > 0 && char.IsLetter(name[0]) &&
                    name.Replace("_", "").All(ch => char.IsLetterOrDigit(ch) || ch == '_');
                if (validName)
                {
                    string value = Evaluate(expression.Substring(eqIdx + 1));
                    if (value.StartsWith("ERR")) return value;
                    _variables[name.ToLower()] = Clean(value);
                    return Clean(value);
                }
            }

            _expr = expression;
            _pos = 0;
            try
            {
                return Clean(ParseExpr());
            }
            catch (CalcException ex)
            {
                return "ERR: " + ex.Message;
            }
            catch (Exception ex)
            {
                return "ERR: " + ex.Message;
            }
        }

        /// <summary>
        /// Сбрасывает все пользовательские переменные (оставляет только встроенные
        /// константы). Полезно перед запуском новой сессии вычислений.
        /// </summary>
        public static void ResetVariables()
        {
            // Удаляем все ключи, кроме встроенных констант
            var keysToRemove = new List<string>();
            foreach (var kv in _variables)
            {
                string k = kv.Key;
                if (k != "pi" && k != "e" && k != "phi" && k != "tau" &&
                    k != "ln2" && k != "ln10" &&
                    k != "c" && k != "g" && k != "h" && k != "na" && k != "r" && k != "k")
                {
                    keysToRemove.Add(k);
                }
            }
            foreach (string k in keysToRemove)
                _variables.Remove(k);
        }
    }
}
