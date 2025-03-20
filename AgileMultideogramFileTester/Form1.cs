using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AgileMultiIdeogramFileTester
{
    public partial class Form1 : Form
    {
        private struct AffyIndexes
        {
            public bool OK;
            public int RSIndex;
            public int GenotypeIndex;
            public int ChromosomeIndex;
            public int Position;
        }

        private struct VCFIndexes
        {
            public bool OK;
            public int ID;
            public int REF;
            public int ALT;
            public int CHROM;
            public int POS;
            public int FORMAT;
            public int length;
            public int biggest;
        }

        int[] chromosomeCounts = null;
        public Form1()
        {
            InitializeComponent();
            cboType.SelectedIndex = 0;
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSelect.Enabled = (cboType.SelectedIndex != 0);
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {

            string filename = FileString.OpenAs("Select the file to test", getFileExtension());
            if (System.IO.File.Exists(filename) == false)
            { return; }

            txtAnswer.Clear();
            Application.DoEvents();
            resetChromosomeCounts();

            if (cboType.Text == "Affymetrix birdseed (tab-delimited - *.txt)")
            { txtAnswer.Text = testBirdseed(filename) + chromosomeCountData(); }
            else if (cboType.Text == "Affymetrix Excel (tab-delimited - *.xls)")
            { txtAnswer.Text = testAffy(filename) + chromosomeCountData(); }
            else if (cboType.Text == "VCF")
            { txtAnswer.Text = testVCF(filename) + chromosomeCountData(); }
            else if (cboType.Text == "g.VCF")
            { txtAnswer.Text = testgVCF(filename) + chromosomeCountData(); }

        }

        private void resetChromosomeCounts()
        {
            chromosomeCounts = new int[104];
        }

        private string chromosomeCountData()
        {
            int padding = 0;
            StringBuilder sb = new StringBuilder();
            for (int index = 1; index < 101; index++)
            {
                if (chromosomeCounts[index].ToString("N0").Length > padding)
                { padding = chromosomeCounts[index].ToString("N0").Length; }
            }

            for (int index = 1; index < 101; index++)
            {
                if (chromosomeCounts[index] > 0)
                { sb.Append(chromosomeCounts[index].ToString("N0").PadLeft(padding) + " " + "Chromosome " + index.ToString() + "\r\n"); }
            }
            if (chromosomeCounts[101] > 0)
            { sb.Append(chromosomeCounts[101].ToString("N0").PadLeft(padding) + " " + "Chromosome X" + "\r\n"); }
            if (chromosomeCounts[102] > 0)
            { sb.Append(chromosomeCounts[102].ToString("N0").PadLeft(padding) + " " + "Chromosome Y" + "\r\n"); }
            if (chromosomeCounts[103] > 0)
            { sb.Append(chromosomeCounts[103].ToString("N0").PadLeft(padding) + " " + "Chromosome MT" + "\r\n"); }

            return "\r\n\r\nChromosome SNP count\r\n" +sb.ToString();

        }

        private string testVCF(string filename)
        {
            VCFIndexes result = new VCFIndexes();


            result.CHROM = -1;
            result.POS = -1;
            result.ID = -1;
            result.REF = -1;
            result.ALT = -1;
            result.FORMAT = -1;
            result.length = -1;

            gVCFReader fs = null;
            string answer = "";
            try
            {
                fs = new gVCFReader(filename);
                string[] strRow = null;
                while (fs.Peek() > 0)
                {
                    string line = fs.ReadLine();
                    if (line.StartsWith("#CHROM") == true)
                    { strRow = line.Split('\t'); break; }
                }

                if (strRow != null)
                {
                    result.length = strRow.Length;
                    for (int intStrArray = 0; intStrArray < strRow.Length; intStrArray++)
                    {
                        if (strRow[intStrArray].Trim().ToUpper().Equals("#CHROM") == true)
                        {
                            result.CHROM = intStrArray;
                            if (intStrArray > result.biggest) { result.biggest = intStrArray; }
                        }
                        else if (strRow[intStrArray].Trim().ToUpper().Equals("POS".ToUpper()))
                        {
                            result.POS = intStrArray;
                            if (intStrArray > result.biggest) { result.biggest = intStrArray; }
                        }
                        else if (strRow[intStrArray].Trim().ToUpper().Equals("ID") == true)
                        {
                            result.ID = intStrArray;
                            if (intStrArray > result.biggest) { result.biggest = intStrArray; }
                        }
                        else if (strRow[intStrArray].Trim().ToUpper().IndexOf("REF") > -1)
                        {
                            result.REF = intStrArray;
                            if (intStrArray > result.biggest) { result.biggest = intStrArray; }
                        }
                        else if (strRow[intStrArray].Trim().ToUpper().IndexOf("ALT") > -1)
                        {
                            result.ALT = intStrArray;
                            if (intStrArray > result.biggest) { result.biggest = intStrArray; }
                        }
                        else if (strRow[intStrArray].Trim().ToUpper().IndexOf("FORMAT") > -1)
                        {
                            result.FORMAT = intStrArray;
                            if (intStrArray + 1 > result.biggest) { result.biggest = intStrArray + 1; }
                        }
                    }
                }

                if (result.length == -1)
                { answer = "Did not find the line describing the fields\r\n"; }
                else if (result.length > 10)
                { answer = "It appears there are multiple samples in the file, only the first will be used"; }

                int test = 0;
                if (result.length > 9)
                {
                    if (result.CHROM == -1)
                    { answer = "No chromosome column\r\n"; test++; }
                    if (result.POS == -1)
                    { answer += "No position column\r\n"; test++; }
                    if (result.ID == -1)
                    { answer += "No SNP name column\r\n"; test++; }
                    if (result.ALT == -1)
                    { answer += "No Alternative allele column\r\n"; test++; }
                    if (result.REF == -1)
                    { answer += "No Reference allele column\r\n"; test++; }
                    if (result.FORMAT == -1)
                    { answer += "No Format column\r\n"; test++; }
                    if (result.length != 10)
                    { answer += "There is not 10 columns: " + result.length.ToString() + "\r\n"; }

                }

                fs?.Close();

                if (test == 0 && result.FORMAT == result.length - 2)
                {
                    result.OK = true;
                    answer = CountErrorsVCF(filename, result);
                }
            }
            catch (Exception ex)
            {
                answer += "\r\n\r\n An error occurred while reading the file: \r\n" + ex.Message;

            }
            finally { fs?.Close(); }

            return answer;
        }

        private string CountErrorsVCF(string fileName, VCFIndexes indexes)
        {
            gVCFReader fs = null;
            string answer = "";
            int counter = 0;
            int chromosomeCount = 0;
            int positionCount = 0;
            int REFCount = 0;
            int ALTCount = 0;
            int RSCount = 0;
            int FORMATCount = 0;
            int GenotypeCount = 0;
            int ADCount = 0;
            int DPCount = 0;
            int toShort = 0;
            int toShortFormat = 0;

            try
            {
                fs = new gVCFReader(fileName);

                while (fs.Peek() > 0)
                {
                    string[] items = fs.ReadLine().Split('\t');
                    if (items[0].StartsWith("#") != true)
                    {
                        counter++;
                        if (items.Length > indexes.biggest)
                        {
                            items[indexes.CHROM] = items[indexes.CHROM].ToLower().Replace("chr", "");
                            switch (items[indexes.CHROM].ToLower())
                            {
                                case "mt":
                                case "m":
                                    chromosomeCounts[103]++;
                                    chromosomeCount++;
                                    break;
                                case "x":
                                    chromosomeCounts[101]++;
                                    chromosomeCount++;
                                    break;
                                case "y":
                                    chromosomeCounts[102]++;
                                    chromosomeCount++;
                                    break;
                                default:
                                    if (int.TryParse(items[indexes.CHROM], out int number))
                                    {
                                        if (number >= 1 && number <= 100)
                                        {
                                            chromosomeCounts[number]++;
                                            chromosomeCount++; 
                                        }
                                    }
                                    break;
                            }
                            if (int.TryParse(items[indexes.POS], out int numberP)) { positionCount++; }
                            if (items[indexes.REF].Length == 1) { REFCount++; }
                            if (items[indexes.ALT].Length == 1) { ALTCount++; }
                            if (items[indexes.ID].Length > 2) { RSCount++; }
                            string[] formatID = items[indexes.FORMAT].Split(':');
                            string[] values = items[indexes.FORMAT + 1].Split(':');
                            if (values.Length == formatID.Length)
                            {
                                for (int index = 0; index < values.Length; index++)
                                {
                                    if (formatID[index] == "GT")
                                    { if (values[index] == "0/1" || values[index] == "1/1" || values[index] == "1/0") { GenotypeCount++; } }
                                    else if (formatID[index] == "AD") { ADCount++; }
                                    else if (formatID[index] == "DP") { DPCount++; }
                                }
                            }
                            else
                            { toShortFormat++; }
                        }
                        else
                        { toShort++; }
                    }
                }
            }
            catch (Exception ex) { answer = ex.Message; }
            finally { fs?.Close(); }

            if (string.IsNullOrEmpty(answer) == false)
            {
                answer = "The error below occurred while reading the file:\r\n" + answer + "\r\n\r\n";
            }
           
            int pad = 8;
            if (counter.ToString("N0").Length > 7)
            { pad = counter.ToString("N0").Length + 1; }

            answer += counter.ToString("N0").PadLeft(pad - 1).PadRight(pad * 2) + "lines were read and\r\n";
            answer += paddedNumber(toShort,counter,pad) + "did not contain all the columns\r\n";
            answer += paddedNumber(toShortFormat,counter,pad) + "has different length format and sample data fields\r\n";
            answer += paddedNumber(chromosomeCount,counter,pad) + "has a chromosome ID\r\n";
            answer += paddedNumber(positionCount,counter,pad) + "has a SNP position\r\n";
            answer += paddedNumber(REFCount,counter,pad) + "has a single base reference allele\r\n";
            answer += paddedNumber(ALTCount,counter,pad) + "has a single base alternative allele\r\n";
            answer += paddedNumber(RSCount,counter,pad) + "has a ID\r\n";
            answer += paddedNumber(GenotypeCount,counter,pad) + "has an non-wildtype genotype\r\n";
            answer += paddedNumber(ADCount,counter,pad) + "has a allele read depth count\r\n";
            answer += paddedNumber(DPCount,counter,pad) + "has a read depth count\r\n";

            return answer;
        }

        private string testgVCF(string filename)
        {
            VCFIndexes result = new VCFIndexes();


            result.CHROM = -1;
            result.POS = -1;
            result.ID = -1;
            result.REF = -1;
            result.ALT = -1;
            result.FORMAT = -1;
            result.length = -1;

            gVCFReader fs = null;
            string answer = "";
            try
            {
                fs = new gVCFReader(filename);
                string[] strRow = null;
                while (fs.Peek() > 0)
                {
                    string line = fs.ReadLine();
                    if (line.StartsWith("#CHROM") == true)
                    { strRow = line.Split('\t'); break; }
                }

                if (strRow != null)
                {
                    result.length = strRow.Length;
                    for (int intStrArray = 0; intStrArray < strRow.Length; intStrArray++)
                    {
                        if (strRow[intStrArray].Trim().ToUpper().Equals("#CHROM") == true)
                        {
                            result.CHROM = intStrArray;
                            if (intStrArray > result.biggest) { result.biggest = intStrArray; }
                        }
                        else if (strRow[intStrArray].Trim().ToUpper().Equals("POS".ToUpper()))
                        {
                            result.POS = intStrArray;
                            if (intStrArray > result.biggest) { result.biggest = intStrArray; }
                        }
                        else if (strRow[intStrArray].Trim().ToUpper().Equals("ID") == true)
                        {
                            result.ID = intStrArray;
                            if (intStrArray > result.biggest) { result.biggest = intStrArray; }
                        }
                        else if (strRow[intStrArray].Trim().ToUpper().IndexOf("REF") > -1)
                        {
                            result.REF = intStrArray;
                            if (intStrArray > result.biggest) { result.biggest = intStrArray; }
                        }
                        else if (strRow[intStrArray].Trim().ToUpper().IndexOf("ALT") > -1)
                        {
                            result.ALT = intStrArray;
                            if (intStrArray > result.biggest) { result.biggest = intStrArray; }
                        }
                        else if (strRow[intStrArray].Trim().ToUpper().IndexOf("FORMAT") > -1)
                        {
                            result.FORMAT = intStrArray;
                            if (intStrArray + 1 > result.biggest) { result.biggest = intStrArray + 1; }
                        }
                    }
                }

                if (result.length == -1)
                { answer = "Did not find the line describing the fields\r\n"; }
                else if (result.length > 10)
                { answer = "It appears there are multiple samples in the file, only the first will be used"; }

                int test = 0;
                if (result.length > 9)
                {
                    if (result.CHROM == -1)
                    { answer = "No chromosome column\r\n"; test++; }
                    if (result.POS == -1)
                    { answer += "No position column\r\n"; test++; }
                    if (result.ID == -1)
                    { answer += "No SNP name column\r\n"; test++; }
                    if (result.ALT == -1)
                    { answer += "No Alternative allele column\r\n"; test++; }
                    if (result.REF == -1)
                    { answer += "No Reference allele column\r\n"; test++; }
                    if (result.FORMAT == -1)
                    { answer += "No Format column\r\n"; test++; }
                    if (result.length != 10)
                    { answer += "There is not 10 columns: " + result.length.ToString() + "\r\n"; }

                }

                fs?.Close();

                if (test == 0 && result.FORMAT == result.length - 2)
                {
                    result.OK = true;
                    answer = CountErrorsgVCF(filename, result);
                }
            }
            catch (Exception ex)
            {
                answer += "\r\n\r\n An error occurred while reading the file: \r\n" + ex.Message;

            }
            finally { fs?.Close(); }

            return answer;
        }

        private string CountErrorsgVCF(string fileName, VCFIndexes indexes)
        {
            gVCFReader fs = null;
            string answer = "";
            int counter = 0;
            int chromosomeCount = 0;
            int positionCount = 0;
            int REFCount = 0;
            int ALTCount = 0;
            int RSCount = 0;
            int FORMATCount = 0;
            int GenotypeCount = 0;
            int ADCount = 0;
            int DPCount = 0;
            int toShort = 0;
            int toShortFormat = 0;

            try
            {
                fs = new gVCFReader(fileName);

                while (fs.Peek() > 0)
                {
                    string[] items = fs.ReadLine().Split('\t');
                    if (items[0].StartsWith("#") != true)
                    {
                        counter++;
                        if (items.Length > indexes.biggest)
                        {
                            items[indexes.CHROM] = items[indexes.CHROM].ToLower().Replace("chr", "");
                            switch (items[indexes.CHROM].ToLower())
                            {
                                case "mt":
                                case "m":
                                    chromosomeCounts[103]++;
                                    chromosomeCount++;
                                    break;
                                case "x":
                                    chromosomeCounts[101]++;
                                    chromosomeCount++;
                                    break;
                                case "y":
                                    chromosomeCounts[102]++;
                                    chromosomeCount++;
                                    break;
                                default:
                                    if (int.TryParse(items[indexes.CHROM], out int number))
                                    {
                                        if (number >= 1 && number <= 100)
                                        {
                                            chromosomeCounts[number]++;
                                            chromosomeCount++;
                                        }
                                    }
                                    break;
                            }
                            if (int.TryParse(items[indexes.POS], out int numberP)) { positionCount++; }
                            if (items[indexes.REF].Length == 1) { REFCount++; }
                            if (items[indexes.ALT].Length == 1 || (items[indexes.ALT].Length > 2 && items[indexes.ALT].Substring(1,2) == ",<") )
                            { ALTCount++; }
                            if (items[indexes.ID].Length > 2) { RSCount++; }
                            string[] formatID = items[indexes.FORMAT].Split(':');
                            string[] values = items[indexes.FORMAT + 1].Split(':');
                            if (values.Length == formatID.Length)
                            {
                                for (int index = 0; index < values.Length; index++)
                                {
                                    if (formatID[index] == "GT")
                                    { if (values[index] == "0/1" || values[index] == "1/1" || values[index] == "1/0") { GenotypeCount++; } }
                                    else if (formatID[index] == "AD") { ADCount++; }
                                    else if (formatID[index] == "DP") { DPCount++; }
                                }
                            }
                            else
                            { toShortFormat++; }
                        }
                        else
                        { toShort++; }
                    }
                }
            }
            catch (Exception ex) { answer = ex.Message; }
            finally { fs?.Close(); }

            if (string.IsNullOrEmpty(answer) == false)
            {
                answer = "The error below occurred while reading the file:\r\n" + answer + "\r\n\r\n";
            }

            int pad = 8;
            if (counter.ToString("N0").Length > 7)
            { pad = counter.ToString("N0").Length + 1; }

            answer += counter.ToString("N0").PadLeft(pad - 1).PadRight(pad*2) + "lines were read and\r\n";
            answer += paddedNumber(toShort,counter,pad) + "did not contain all the columns\r\n";
            answer += paddedNumber(toShortFormat, counter, pad) + "has different length format and sample data fields\r\n";
            answer += paddedNumber(chromosomeCount, counter, pad) + "has a chromosome ID\r\n";
            answer += paddedNumber(positionCount, counter, pad) + "has a SNP position\r\n";
            answer += paddedNumber(REFCount, counter, pad) + "has a single base reference allele\r\n";
            answer += paddedNumber(ALTCount, counter, pad) + "has a single base alternative allele\r\n";
            answer += paddedNumber(RSCount, counter, pad) + "has a ID\r\n";
            answer += paddedNumber(GenotypeCount, counter, pad) + "had an non-wildtype genotype\r\n";
            answer += paddedNumber(ADCount, counter, pad) + "has allele read depth data\r\n";
            answer += paddedNumber(DPCount, counter, pad) + "has a read depth count\r\n";

            return answer;
        }

        private string paddedNumber(int number,int counts, int padding)
        {
            double percent = (double)number * 100 / counts;
            string answer = (percent.ToString("N2") + "%").PadLeft(padding - 1) + " " + number.ToString("N0").PadLeft(padding - 1) + " ";
            return answer;
        }

        private string testAffy(string filename)
        {
            AffyIndexes result = new AffyIndexes();
            int intStrArray = -1;
            string strDataArray;

            result.ChromosomeIndex = -1;
            result.GenotypeIndex = -1;
            result.Position = -1;
            result.RSIndex = -1;


            gVCFReader fs = null;
            string answer = "";
            try
            {
                fs = new gVCFReader(filename);

                for (int index = 0; index < 10; index++)
                {
                    if (fs.Peek() > 0)
                    {
                        string[] strRow = fs.ReadLine().Split('\t');

                        for (intStrArray = 0; intStrArray < strRow.Length; intStrArray++)
                        {
                            if (strRow[intStrArray].Trim().ToUpper().Equals("Chromosome".ToUpper()) == true)
                            { result.ChromosomeIndex = intStrArray; }
                            else if (strRow[intStrArray].Trim().ToUpper().Equals("Physical Position".ToUpper()))
                            { result.Position = intStrArray; }
                            else if (strRow[intStrArray].Trim().ToUpper().Equals("dbSNP RS ID".ToUpper()) == true)
                            { result.RSIndex = intStrArray; }
                            else if (strRow[intStrArray].Trim().ToUpper().IndexOf("CALL") > -1)
                            { result.GenotypeIndex = intStrArray; }
                        }
                    }
                }

                if (result.ChromosomeIndex == -1)
                { answer = "No chromosome column\r\n"; }
                if (result.Position == -1)
                { answer += "No position column\r\n"; }
                if (result.RSIndex == -1)
                { answer += "No SNP name column\r\n"; }
                if (result.GenotypeIndex == -1)
                { answer += "No genotype column\r\n"; }

                fs?.Close();

                if (string.IsNullOrEmpty(answer) == true)
                {
                    result.OK = true;
                    answer = CountErrorsAffy(filename, result);
                }
            }
            catch (Exception ex)
            {
                answer += "\r\n\r\n An error occurred while reading the file: \r\n" + ex.Message;

            }
            finally { fs?.Close(); }

            return answer;
        }

        private string CountErrorsAffy(string fileName, AffyIndexes indexes)
        {
            gVCFReader fs = null;
            string answer = "";
            int counter = 0;
            int chromosomeCount = 0;
            int positionCount = 0;
            int genotypeCount = 0;
            int RSCount = 0;
            int toShort = 0;

            try
            {
                fs = new gVCFReader(fileName);

                while (fs.Peek() > 0)
                {
                    string[] items = fs.ReadLine().Split('\t');
                    counter++;
                    if (items.Length > 3)
                    {

                        items[indexes.ChromosomeIndex] = items[indexes.ChromosomeIndex].ToLower().Replace("chr", "");
                        switch (items[indexes.ChromosomeIndex].ToLower())
                        {
                            case "mt":
                            case "m":
                                chromosomeCounts[103]++;
                                chromosomeCount++;
                                break;
                            case "x":
                                chromosomeCounts[101]++;
                                chromosomeCount++;
                                break;
                            case "y":
                                chromosomeCounts[102]++;
                                chromosomeCount++;
                                break;
                            default:
                                if (int.TryParse(items[indexes.ChromosomeIndex], out int number))
                                {
                                    if (number >= 1 && number <= 100)
                                    {
                                        chromosomeCounts[number]++;
                                        chromosomeCount++;
                                    }
                                }
                                break;
                        }
                        if (int.TryParse(items[indexes.Position], out int numberP)) { positionCount++; }
                        items[indexes.GenotypeIndex] = items[indexes.GenotypeIndex].ToLower();
                        if (items[indexes.GenotypeIndex] == "aa" || items[indexes.GenotypeIndex] == "bb" || items[indexes.GenotypeIndex] == "ab")
                        { genotypeCount++; }
                        if (items[indexes.RSIndex].Length > 2) { RSCount++; }
                    }
                    else
                    { toShort++; }
                }
            }
            catch (Exception ex) { answer = ex.Message; }
            finally { fs?.Close(); }

            if (string.IsNullOrEmpty(answer) == false)
            {
                answer = "The error below occurred while reading the file:\r\n" + answer + "\r\n\r\n";
            }

            int pad = 8;
            if (counter.ToString("N0").Length > 7)
            { pad = counter.ToString("N0").Length + 1; }

            answer += counter.ToString("N0").PadLeft(pad - 1).PadRight(pad * 2) + "lines were read and\r\n";
            answer += paddedNumber(toShort, counter, pad) + "did not contain all the columns\r\n";
            answer += paddedNumber(chromosomeCount, counter, pad) + "has a chromosome ID\r\n";
            answer += paddedNumber(positionCount, counter, pad) + "did not contain all the columns\r\n";
            answer += paddedNumber(genotypeCount, counter, pad) + "did not contain all the columns\r\n";
            answer += paddedNumber(RSCount, counter, pad) + "did not contain all the columns\r\n";

            return answer;
        }

        private string testBirdseed(string filename)
        {
            AffyIndexes result = new AffyIndexes();
            int intStrArray = -1;
            string strDataArray;

            result.ChromosomeIndex = -1;
            result.GenotypeIndex = -1;
            result.Position = -1;
            result.RSIndex = -1;


            gVCFReader fs = null;
            string answer = "";
            try
            {
                fs = new gVCFReader(filename);

                for (int index = 0; index < 10; index++)
                {
                    if (fs.Peek() > 0)
                    {
                        string[] strRow = fs.ReadLine().Split('\t');

                        for (intStrArray = 0; intStrArray < strRow.Length; intStrArray++)
                        {
                            if (strRow[intStrArray].Trim().ToUpper().Equals("Chromosome".ToUpper()) == true)
                            { result.ChromosomeIndex = intStrArray; }
                            else if (strRow[intStrArray].Trim().ToUpper().Equals("Chromosomal Position".ToUpper()))
                            { result.Position = intStrArray; }
                            else if (strRow[intStrArray].Trim().ToUpper().Equals("dbSNP RS ID".ToUpper()) == true)
                            { result.RSIndex = intStrArray; }
                            else if (strRow[intStrArray].Trim().ToUpper().IndexOf("CALL") > -1)
                            { result.GenotypeIndex = intStrArray; }
                        }
                    }
                }

                if (result.ChromosomeIndex == -1)
                { answer = "No chromosome column\r\n"; }
                if (result.Position == -1)
                { answer += "No position column\r\n"; }
                if (result.RSIndex == -1)
                { answer += "No SNP name column\r\n"; }
                if (result.GenotypeIndex == -1)
                { answer += "No genotype column\r\n"; }

                fs?.Close();

                if (string.IsNullOrEmpty(answer) == true)
                {
                    result.OK = true;
                    answer = CountErrorsBirdseed(filename, result);
                }
            }
            catch (Exception ex)
            {
                answer += "\r\n\r\n An error occurred while reading the file: \r\n" + ex.Message;

            }
            finally { fs?.Close(); }

            return answer;
        }

        private string CountErrorsBirdseed(string fileName, AffyIndexes indexes)
        {
            gVCFReader fs = null;
            string answer = "";
            int counter = 0;
            int chromosomeCount = 0;
            int positionCount = 0;
            int genotypeCount = 0;
            int RSCount = 0;
            int toShort = 0;

            try
            {
                fs = new gVCFReader(fileName);

                while (fs.Peek() > 0)
                {
                    string[] items = fs.ReadLine().Split('\t');
                    counter++;
                    if (items.Length > 3)
                    {

                        items[indexes.ChromosomeIndex] = items[indexes.ChromosomeIndex].ToLower().Replace("chr", "");
                        switch (items[indexes.ChromosomeIndex].ToLower())
                        {
                            case "mt":
                            case "m":
                                chromosomeCounts[103]++;
                                chromosomeCount++;
                                break;
                            case "x":
                                chromosomeCounts[101]++;
                                chromosomeCount++;
                                break;
                            case "y":
                                chromosomeCounts[102]++;
                                chromosomeCount++;
                                break;
                            default:
                                if (int.TryParse(items[indexes.ChromosomeIndex], out int number))
                                {
                                    if (number >= 1 && number <= 100)
                                    {
                                        chromosomeCounts[number]++;
                                        chromosomeCount++;
                                    }
                                }
                                break;
                        }
                        if (int.TryParse(items[indexes.Position], out int numberP)) { positionCount++; }
                        items[indexes.GenotypeIndex] = items[indexes.GenotypeIndex].ToLower();
                        if (items[indexes.GenotypeIndex] == "aa" || items[indexes.GenotypeIndex] == "bb" || items[indexes.GenotypeIndex] == "ab")
                        { genotypeCount++; }
                        if (items[indexes.RSIndex].Length > 2) { RSCount++; }
                    }
                    else
                    { toShort++; }
                }
            }
            catch (Exception ex) { answer = ex.Message; }
            finally { fs?.Close(); }

            if (string.IsNullOrEmpty(answer) == false)
            {
                answer = "The error below occurred while reading the file:\r\n" + answer + "\r\n\r\n";
            }

            int pad = 8;
            if (counter.ToString("N0").Length > 7)
            { pad = counter.ToString("N0").Length + 1; }

            answer += counter.ToString("N0").PadLeft(pad - 1).PadRight(pad * 2) + "lines were read and\r\n";
            answer += paddedNumber(toShort, counter, pad) + "did not contain all the columns\r\n";
            answer += paddedNumber(chromosomeCount, counter, pad) + "has a chromosome ID\r\n";
            answer += paddedNumber(positionCount, counter, pad) + "has a SNP position\r\n";
            answer += paddedNumber(genotypeCount, counter, pad) + "has a genotype ID call\r\n";
            answer += paddedNumber(RSCount, counter, pad) + "has a RS ID\r\n";

            return answer;
        }

        //private string testregions(string filename)
        //{ }



        private string getFileExtension()
        {
            string answer = ("");

            switch (cboType.Text)
            {
                case "VCF":
                    answer = "VCF file (*.vcf;*.vcf.gz)|*.vcf;*.vcf.gz";
                    break;
                case "g.VCF":
                    answer = "Genomic VCF file (*.g.vcf;*.g.vcf.gz)|*.g.vcf;*.g.vcf.gz";
                    break;
                case "Affymetrix Excel (tab-delimited - *.xls)":
                    answer = "Original Affymetrix format (*.xls;*.xls.gz)|*.xls;*.xls.gz";
                    break;
                case "Affymetrix birdseed (tab-delimited - *.txt)":
                    answer = "Affymetrix birdseed (*.txt;*.txt.gz)|*.txt;*.txt.gz";
                    break;                
            }

            return answer;
        }
    }
}
