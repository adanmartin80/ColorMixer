// See https://aka.ms/new-console-template for more information

using System;
using System.Drawing;
using System.Globalization;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Prusaslicer // Note: actual namespace depends on the project name.
{
    internal class Mixer
    {
        const string _wildcard = ";py$";
        const string _gcodeLinearMove = "G1";
        const string _gcodeSetMixed_0 = "M163 S0 P";
        const string _gcodeSetMixed_1 = "M163 S1 P";
        const string _gcodeSaveMixed = "M164 S0";
        const string _gcodeExtrudeChar = "E";

        static void Main(string[] args)
        {
            if (args == null || args.Length <= 0) 
                throw new ArgumentException("No se ha pasado ningún argumento.\nEs necesario pasar como argumento el path del fichero gcode.");

            var path = args[0];
            if (!File.Exists(path))
                throw new FileNotFoundException($"No se puede localizar el fichero con la ruta {path}");

            var file = new FileInfo(path);

            InsertIntoGcode(path, _wildcard);

        }

        static void InsertIntoGcode(string path, string comodin)
        {
            var sbFileOutput = new StringBuilder();
            var GcodeFile = new FileInfo(path);

            using (var fs = new StreamReader(GcodeFile.OpenRead()))
            {
                (double WhiteColor, double BlackColor) colormixed = default;

                while (true)
                {
                    var line = fs.ReadLine();
                    if (line == null) break;

                    if (!line.StartsWith(comodin, StringComparison.InvariantCultureIgnoreCase))
                    {
                        sbFileOutput.AppendLine(line);
                        continue;
                    }

                    if (line.Contains(_gcodeLinearMove, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var extrudefactor = AddExtrudeFactor(comodin, line, colormixed);
                        sbFileOutput.Append(extrudefactor);
                    }
                    else
                    {
                        colormixed = AddColorMixed(comodin, line);
                        var set_mix_factor_0 = $"{_gcodeSetMixed_0}{colormixed.BlackColor.ToString("0.0", CultureInfo.CreateSpecificCulture("en-CA"))}";
                        var set_mix_factor_1 = $"{_gcodeSetMixed_1}{colormixed.WhiteColor.ToString("0.0", CultureInfo.CreateSpecificCulture("en-CA"))}";

                        sbFileOutput.AppendLine(set_mix_factor_0);
                        sbFileOutput.AppendLine(set_mix_factor_1);
                        sbFileOutput.AppendLine(_gcodeSaveMixed);
                    }
                }

                fs.Close();
            }

            File.WriteAllText(path, sbFileOutput.ToString());
        }

        private static StringBuilder AddExtrudeFactor(string comodin, string gcode, (double WhiteColor, double BlackColor)? colormixed)
        {
            if (!colormixed.HasValue) return new StringBuilder();

            var factor = colormixed.Value.WhiteColor / 2;
            var result = new StringBuilder();
            var error = false;

            var sentence = gcode[comodin.Length..].Trim();
            sentence.Split(" ").ToList().ForEach(word => {
                if (!word.StartsWith(_gcodeExtrudeChar, StringComparison.InvariantCultureIgnoreCase)) result.Append($"{word} ");
                else 
                {
                    var extrudeText = word[1..].Trim();
                    double extrude = 0;

                    error = !double.TryParse(extrudeText, out extrude);
                    if (!error)
                    {
                        extrude = (extrude * factor) + extrude;
                        result.Append($"{_gcodeExtrudeChar}{extrude.ToString("0.00000", CultureInfo.CreateSpecificCulture("en-CA"))} ");
                    }
                    else result.Append($"{word} ");
                }
                
            });

            return result;
        }

        private static (double WhiteColor, double BlackColor) AddColorMixed(string comodin, string line)
        {
            var result = new StringBuilder();
            var color = HexToRgb(line, comodin);
            var white_color = Math.Ceiling((((0.3 * color.R) + (0.59 * color.G) + (0.11 * color.B)) / 255f) * 10) / 10;
            var black_color = 1.0 - white_color;

            return (white_color, black_color);
        }

        static Color HexToRgb(string gcode, string comodin)
        {
            var colorHex = gcode[comodin.Length..].Trim();

            var color = (Color?)new ColorConverter().ConvertFromString(colorHex);

            if (!color.HasValue)
                throw new ApplicationException("No se ha podido recuperar el color.");

            return color.Value;
        }


    }
}
