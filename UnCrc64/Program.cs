using System.Diagnostics;
using System.Globalization;


Console.WriteLine("Hello, World!");

foreach (string arg in args)
{
    Console.WriteLine(arg);
}

string hashString = "";
string startString = "";
int minCharacters = 0;
int maxCharacters = int.MaxValue;

bool extendedCharacters = false;

for (int i = 0; i < args.Length; i++)
{
    string arg = args[i];

    if (arg.Equals("-h") || arg.Equals("-hash"))
    {
        hashString = args[i + 1];
    }

    if (arg.Equals("-ea") || arg.Equals("-extendedAlphabet"))
    {
        extendedCharacters = true;
    }

    // if (arg.Equals("-start"))
    // {
    //     startString = args[i + 1];
    // }
    //
    // if (arg.Equals("-min"))
    // {
    //     minCharacters = int.Parse(args[i + 1]);
    // }

    if (arg.Equals("-max"))
    {
        maxCharacters = int.Parse(args[i + 1]);
    }
}

ulong hash;
bool hashWorked;

if (hashString.StartsWith("0x"))
{
    hashWorked = ulong.TryParse(hashString.Remove(0, 2), NumberStyles.AllowHexSpecifier, null, out hash);
}
else
{
    hashWorked = ulong.TryParse(hashString, out hash);
}

 

Console.Out.WriteLine($"Target Hash: {hash}; Max characters: {maxCharacters}");

if (!hashWorked)
{
    Console.Out.WriteLine("Please specify a valid ulong hash! You can copy this from values in Rider.");
    return;
}

if (maxCharacters == int.MaxValue)
{
    Console.Out.WriteLine("Please specify a valid max characters value! You can set this to a high number like 100 if you want to search EVERYTHING vaguely possible.");
    return;
}

string extendedAlphabet = "abcdefghijklmopqurstuvwxyz0123456789 -=_+!@#$%^&*()~`{}|[]\\;':\"<>?,./";

string reducedAlphabet = "abcdefghijklmopqurstuvwxyz0123456789 ";

DateTime start = DateTime.Now;

IEnumerable<string> strings =
    GetAllStrings(extendedCharacters ? extendedAlphabet : reducedAlphabet,
        maxCharacters);

List<string> matches = [];

foreach (string currentString in strings)
{
    // Console.Out.WriteLine(currentString);
    ulong curHash = TelltaleToolKit.Hashing.Crc64.Compute(currentString);
    if (curHash == hash)
    {
        Console.Out.WriteLine("Found matching hashed string!!!!!!");
        Console.Out.WriteLine($"String is: {currentString}");
        matches.Add(currentString);
    }
}

if (matches.Count == 0)
{
    Console.Out.WriteLine($"No matches found in {DateTime.Now - start}");
}
else
{
    Console.Out.WriteLine($"{matches.Count} matches found in {DateTime.Now - start}!");
}



return;

static IEnumerable<string> GetAllStrings(string alphabet, int maxLength)
{
    // Yield the empty string first if desired

    // Generate combinations length by length
    for (int length = 0; length <= maxLength; length++)
    {
        foreach (string str in GenerateCombinations(alphabet, length, ""))
        {
            yield return str;
        }
    }
}

static IEnumerable<string> GenerateCombinations(string alphabet, int length, string current)
{
    // Base case: combination reached target length
    if (current.Length == length)
    {
        yield return current;
        yield break;
    }

    // Recursive case: append each character from alphabet
    foreach (char c in alphabet)
    {
        yield return current + c;
    }

    foreach (char nextC in alphabet)
    {
        foreach (string str in GenerateCombinations(alphabet, length, current + nextC))
        {
            yield return str;
        }
    }
}
