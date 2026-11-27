// Regression test suite for UNLIMMATH.CalcEngine (string-based arbitrary-precision calculator).
// Oracle: System.Numerics.BigInteger for exact integer arithmetic, double for transcendentals.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

static class TestProgram
{
    private static int _pass = 0;
    private static int _fail = 0;
    private static readonly List<string> _failures = new List<string>();

    private static string Eval(string expr)
    {
        return UNLIMMATH.CalcEngine.Evaluate(expr);
    }

    /// <summary>Exact-string test: result must equal expected exactly.</summary>
    private static void T(string name, string expr, string expected)
    {
        string got;
        try { got = Eval(expr); }
        catch (Exception ex) { got = "EX: " + ex.Message; }
        if (got == expected) { _pass++; }
        else { _fail++; _failures.Add(string.Format("FAIL {0}: '{1}' => '{2}', expected '{3}'", name, expr, got, expected)); }
    }

    /// <summary>Numeric approx test: parse result as double, compare within tolerance.</summary>
    private static void TA(string name, string expr, double expected, double tol)
    {
        string got;
        try { got = Eval(expr); }
        catch (Exception ex) { got = "EX: " + ex.Message; }
        double d;
        bool ok = false;
        if (got != null && !got.StartsWith("ERR") && !got.StartsWith("EX") &&
            double.TryParse(got, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
        {
            double diff = Math.Abs(d - expected);
            ok = diff <= tol || (expected != 0 && diff / Math.Abs(expected) <= tol);
        }
        if (ok) { _pass++; }
        else { _fail++; _failures.Add(string.Format("FAIL {0}: '{1}' => '{2}', expected ~{3}", name, expr, got, expected.ToString(CultureInfo.InvariantCulture))); }
    }

    /// <summary>Error-path test: result must start with ERR.</summary>
    private static void TE(string name, string expr)
    {
        string got;
        try { got = Eval(expr); }
        catch (Exception ex) { got = "EX: " + ex.Message; }
        if (got != null && got.StartsWith("ERR")) { _pass++; }
        else { _fail++; _failures.Add(string.Format("FAIL {0}: '{1}' => '{2}', expected ERR", name, expr, got)); }
    }

    private static BigInteger FactBig(int n)
    {
        BigInteger r = 1;
        for (int i = 2; i <= n; i++) r *= i;
        return r;
    }

    static int Main()
    {
        // ------------------ basic arithmetic (exact, oracle BigInteger) ------------------
        T("add", "2+3", "5");
        T("add-fract", "2.5+2.5", "5");
        T("sub", "10-4", "6");
        T("sub-neg", "2-5", "-3");
        T("mul", "3*4", "12");
        T("mul-neg", "-2*3", "-6");
        T("div", "7/2", "3.5");
        T("div-neg", "-6/2", "-3");
        T("div-1-3", "1/3", "0.33333333333333333333");
        T("mul-prec", "0.1+0.2", "0.3");
        T("mul-prec2", "0.3*3", "0.9");
        T("mod", "10 mod 3", "1");
        T("mod-fract", "10.5 mod 3", "1.5");
        T("mod-neg", "-10 mod 3", "-1");
        T("prio", "2+3*4", "14");
        T("prio-paren", "(2+3)*4", "20");
        T("pow-right-assoc", "2^3^2", "512");
        T("pow-neg-exp", "2^-1", "0.5");
        T("pow-zero", "5^0", "1");
        T("ws", "  2 +   3  ", "5");
        T("nesting", "(((((1+2)))))", "3");

        BigInteger big1 = BigInteger.Parse("123456789") * BigInteger.Parse("987654321");
        T("bigint-mul", "123456789*987654321", big1.ToString());
        T("bigint-pow", "2^100", BigInteger.Pow(2, 100).ToString());
        T("fact-20", "fact(20)", FactBig(20).ToString());
        T("fact-25", "fact(25)", FactBig(25).ToString());
        T("fact-0", "fact(0)", "1");
        T("fib-100", "fib(100)", "354224848179261915075");

        // multiplication tricks paths
        T("mul-by5", "5*8", "40");
        T("mul-by5b", "8*5", "40");
        T("mul-by25", "25*4", "100");
        T("mul-by11", "11*11", "121");
        T("mul-99999", "99999*99999", "9999800001");
        T("mul-50x8", "50*8", "400");
        T("mul-zeros", "50*80", "4000");
        T("mul-frac", "0.0000001*10000000", "1");

        // ------------------ constants ------------------
        TA("pi", "pi", Math.PI, 1e-19);
        TA("e", "e", Math.E, 1e-19);
        TA("phi", "phi", (1 + Math.Sqrt(5)) / 2, 1e-19);
        TA("tau", "tau", 2 * Math.PI, 1e-19);
        TA("ln2", "ln2", Math.Log(2), 1e-19);
        TA("ln10", "ln10", Math.Log(10), 1e-19);

        // ------------------ trigonometry ------------------
        TA("sin0", "sin(0)", 0, 1e-15);
        TA("sin-pi6", "sin(pi/6)", 0.5, 1e-15);
        TA("sin-pi2", "sin(pi/2)", 1, 1e-15);
        TA("sin-neg", "sin(-pi/6)", -0.5, 1e-15);
        TA("sin-2pi", "sin(2*pi+pi/6)", 0.5, 1e-15);
        TA("cos0", "cos(0)", 1, 1e-15);
        TA("cos-pi", "cos(pi)", -1, 1e-15);
        TA("tan-pi4", "tan(pi/4)", 1, 1e-12);
        TA("asin-05", "asin(0.5)", Math.PI / 6, 1e-15);
        TA("acos-05", "acos(0.5)", Math.PI / 3, 1e-15);
        TA("atan-1", "atan(1)", Math.PI / 4, 1e-15);
        TA("atan-neg", "atan(-1)", -Math.PI / 4, 1e-15);
        TA("atan2-11", "atan2(1, 1)", Math.PI / 4, 1e-15);
        TA("atan2-neg", "atan2(-1, -1)", -3 * Math.PI / 4, 1e-15);
        TA("atan2-y0", "atan2(0, -1)", Math.PI, 1e-15);

        // ------------------ hyperbolic ------------------
        TA("sinh-1", "sinh(1)", Math.Sinh(1), 1e-15);
        TA("cosh-0", "cosh(0)", 1, 1e-15);
        TA("cosh-1", "cosh(1)", Math.Cosh(1), 1e-15);
        TA("tanh-1", "tanh(1)", Math.Tanh(1), 1e-15);

        // ------------------ logs / exp ------------------
        // ln(e) etc. have last-digit rounding noise at PREC=20 — approx tests
        TA("ln-e", "ln(e)", Math.Log(Math.E), 1e-15);
        TA("log-1000", "log(1000)", 3, 1e-15);
        TA("log2-8", "log2(8)", 3, 1e-15);
        TA("ln-10", "ln(10)", Math.Log(10), 1e-15);
        TA("exp-1", "exp(1)", Math.E, 1e-15);
        TA("exp-2", "exp(2)", Math.Exp(2), 1e-14);
        TA("exp-neg", "exp(-1)", Math.Exp(-1), 1e-15);
        TA("log-8-2", "log(8, 2)", 3, 1e-12);

        // ------------------ powers / roots ------------------
        T("sqrt-16", "sqrt(16)", "4");
        T("sqrt-0", "sqrt(0)", "0");
        TA("sqrt-2", "sqrt(2)", Math.Sqrt(2), 1e-15);
        T("pow-2-10", "pow(2, 10)", "1024");
        TA("root-27-3", "root(27, 3)", 3, 1e-12);
        TA("pow-2-05", "pow(2, 0.5)", Math.Sqrt(2), 1e-15);
        TA("hypot-3-4", "hypot(3, 4)", 5, 1e-15);

        // ------------------ rounding ------------------
        T("floor", "floor(2.7)", "2");
        T("floor-neg", "floor(-2.7)", "-3");
        T("ceil", "ceil(2.1)", "3");
        T("ceil-neg", "ceil(-2.1)", "-2");
        T("trunc-neg", "trunc(-2.7)", "-2");
        T("round-2", "round(2.567, 2)", "2.57");
        T("round-0", "round(2.567)", "3");
        T("abs", "abs(-5)", "5");
        T("sign", "sign(-3)", "-1");
        T("sign-0", "sign(0)", "0");
        T("min", "min(3, 5)", "3");
        T("max", "max(3, 5)", "5");

        // ------------------ combinatorics / number theory ------------------
        T("ncr-5-2", "ncr(5, 2)", "10");
        T("npr-5-2", "npr(5, 2)", "20");
        T("combrep-5-3", "combrep(5, 3)", "35");
        T("permrep-3-2", "permrep(3, 2)", "9");
        T("isprime-97", "isprime(97)", "1");
        T("isprime-100", "isprime(100)", "0");
        T("isprime-2", "isprime(2)", "1");
        T("isprime-1", "isprime(1)", "0");
        T("prime-25", "prime(25)", "97");
        T("prime-1", "prime(1)", "2");
        T("factorize-360", "factorize(360)", "2^3 * 3^2 * 5");
        T("factorize-97", "factorize(97)", "97");
        T("fib-10", "fib(10)", "55");
        T("euler-12", "euler(12)", "4");
        T("sigma-12", "sigma(12)", "28");
        T("gcd-48-18", "gcd(48, 18)", "6");

        // ------------------ statistics ------------------
        T("mean", "mean(1, 2, 3, 4)", "2.5");
        T("median-odd", "median(1, 3, 5)", "3");
        T("median-even", "median(1, 2, 3, 4)", "2.5");
        // sample standard deviation of {2,4,4,4,5,5,7,9} = sqrt(32/7)
        TA("stddev", "stddev(2, 4, 4, 4, 5, 5, 7, 9)", Math.Sqrt(32.0 / 7.0), 1e-15);

        // ------------------ variables ------------------
        T("var-assign", "x = 5", "5");
        T("var-use", "x + 3", "8");
        T("var-assign2", "y = x + 3", "8");
        T("var-use2", "y * 2", "16");
        T("var-shadow", "pi = 3", "3");           // user can shadow constant
        if (Eval("pi") == "3") _pass++; else { _fail++; _failures.Add("FAIL var-pi-check: user variable should shadow constant"); }

        // ------------------ integrate / solve (parser re-entry!) ------------------
        TA("integ-x2", "integrate(x^2, 0, 1)", 1.0 / 3.0, 1e-12);
        TA("solve-x2-2", "solve(x^2 - 2, 1)", Math.Sqrt(2), 1e-10);
        // CRITICAL: operators AFTER integrate/solve must not be lost (parser state bug)
        TA("integ-then-add", "integrate(x^2, 0, 1) + 5", 1.0 / 3.0 + 5, 1e-12);
        TA("solve-then-add", "solve(x^2 - 2, 1) + 1", Math.Sqrt(2) + 1, 1e-10);
        TA("integ-nested", "1 + integrate(x^2, 0, 1)", 1 + 1.0 / 3.0, 1e-12);

        // ------------------ error paths ------------------
        TE("div0", "1/0");
        TE("mod0", "5 mod 0");
        TE("sqrt-neg", "sqrt(-1)");
        TE("ln-neg", "ln(-1)");
        TE("ln-zero", "ln(0)");
        TE("fact-neg", "fact(-1)");
        TE("asin-2", "asin(2)");
        TE("acos-2", "acos(2)");
        TE("unknown-func", "foobar(1)");
        TE("unknown-name", "2 + qwerty");
        TE("bad-syntax", "2 +");
        // NOTE: "   " (whitespace only) → "0" is documented Evaluate() behaviour, not an error.

        // ------------------ parser robustness (iteration 2) ------------------
        TE("leftover-input", "x+y=5");          // «=5» больше не теряется молча
        TE("unclosed-paren", "(1+2");
        TE("unclosed-paren2", "sin(pi");
        TE("double-dot", "1..2");
        T("trailing-newline", "2+3\n", "5");
        T("inner-newline", "2\n+\n3", "5");
        T("unneeded-paren", "(1+2)*3", "9");
        // median сортируется точным сравнением (не через double)
        T("median-big", "median(123456789012345678901234567890, 1, 2)", "2");

        // ------------------ iteration 3: big-argument paths ------------------
        // sqrt огромных чисел: раньше double-приближение переполнялось > 1e308
        // и Ньютон молча выдавал мусор. 10^400 → ровно 10^200.
        T("sqrt-huge", "sqrt(10^400)", BigInteger.Pow(10, 200).ToString());
        T("sqrt-huge2", "sqrt(10^402)", BigInteger.Pow(10, 201).ToString());
        // большие углы: sin(1000), sin(10^6) — приведение по модулю 2π
        TA("sin-1000", "sin(1000)", Math.Sin(1000), 1e-15);
        TA("cos-1000", "cos(1000)", Math.Cos(1000), 1e-15);
        TA("sin-1e6", "sin(1000000)", Math.Sin(1e6), 1e-14);
        TA("sin-neg-big", "sin(-1000)", Math.Sin(-1000), 1e-15);
        // большие логарифмы: ln(10^100) = 100*ln(10)
        TA("ln-1e100", "ln(10^100)", 100 * Math.Log(10), 1e-15);
        TA("ln-tiny", "ln(0.00000001)", Math.Log(1e-8), 1e-15);
        // и большие степени остаются точными
        T("pow-big", "3^333", BigInteger.Pow(3, 333).ToString());

        // ------------------ misc robustness ------------------
        T("zero", "0", "0");
        T("neg-zero", "-0", "0");
        T("mul-by-1", "1*999999", "999999");
        T("mul-by-0", "12345*0", "0");
        T("div-zero-num", "0/5", "0");
        T("leading-zeros", "007 + 1", "8");
        T("double-dot-num", "0.50 + 0.50", "1");

        // ------------------ results ------------------
        Console.WriteLine("PASS: " + _pass + "  FAIL: " + _fail);
        if (_failures.Count > 0)
        {
            Console.WriteLine("---- FAILURES ----");
            foreach (string f in _failures) Console.WriteLine(f);
            return 1;
        }
        Console.WriteLine("ALL TESTS PASSED");
        return 0;
    }
}
