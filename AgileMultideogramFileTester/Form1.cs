using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

            if (cboType.Text == "Affymetrix birdseed (tab-delimited - *.txt)")
            { txtAnswer.Text = testBirdseed(filename); }


        }

        //private string testVCF(string filename)
        //{ }

        //private string testgVCF(string filename)
        //{ }

        //private string testAffy(string filename)
        //{ }

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
                            case "x":
                            case "y":
                                chromosomeCount++;
                                break;
                            default:
                                if (int.TryParse(items[indexes.ChromosomeIndex], out int number))
                                {
                                    if (number >= 1 && number <= 22)
                                    { chromosomeCount++; }
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

            answer += counter.ToString() + "\tlines were read and\r\n";
            answer = toShort.ToString() + "\tdid not contain all the columns\r\n";
            answer = chromosomeCount.ToString() + "\thad a chromosome ID\r\n";
            answer = positionCount.ToString() + "\thad a position ID\r\n";
            answer = genotypeCount.ToString() + "\thad a genotype ID\r\n";
            answer = RSCount.ToString() + "\thad a RS ID\r\n";

            return answer;
        }

        //private string testregions(string filename)
        //{ }



        private string getFileExtension()
        {
            string answer = ("*.*");

            switch (cboType.Text)
            {
                case "VCF":
                    answer = "VCF file (*.vcf;*.vcf.gz)|*.vcf;*.vcf.gz";
                    break;
                case "g.VCF":
                    answer = "Genomic VCF file (*.g.vcf;*.g.vcf.gz)|*.g.vcf;*.g.vcf.gz";
                    break;
                case "Affymetrix Excel (tab - delimited - *.xls)":
                    answer = "Original Affymetrix format (*.xls)|*.xls";
                    break;
                case "Affymetrix birdseed (tab-delimited - *.txt)":
                    answer = "Affymetrix birdseed (*.txt)|*.txt";
                    break;
                case "Regions file(tab-delimited - *.txt)":
                    answer = "Tab-delimited text file (*.txt)|*.txt";
                    break;
            }

            return answer;
        }
    }
}
