using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OrarioScolastico
{
    public partial class Main : Form
    {
        List<Attivita> _attivitaSospese = new List<Attivita>();
        List<List<Attivita>> listListeClassi = new List<List<Attivita>>();
        List<Attivita[,]> listMatrici = new List<Attivita[,]>();

        const int sizeH = 30;
        const int sizeW = 140;
        const int marginLeft = 15;
        const int marginTop = 135;
        const int ore = 5;
        const int giorni = 6;

        int nClick = 0;
        int[] locButClicked = new int[4];
        const int tentativi = giorni * ore;
        List<string> elencoOre = new List<string>();
        List<string> elencoProf = new List<string>();
        List<string> elencoProfPrec = new List<string>();
        DialogResult result;

        public Main()
        {
            InitializeComponent();
            result = MessageBox.Show("Vuoi generare un nuovo orario?" + Environment.NewLine + "Si: Potrai creare un orario da zero." + Environment.NewLine + "No: Potrai aggiungere classi all'orario già esistente.", "Genera orario?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
                allNotEnable();            
            else {
                string filePath = Path.GetFullPath("OreAttivita.txt");
                StreamReader reader = new StreamReader(filePath);
                string[] righeFile = reader.ReadToEnd().Split(new string[] {"\r\n"}, StringSplitOptions.None);
                reader.Close();

                for (int i = 0; i < righeFile.Length; i++) {
                    if (righeFile[i].Length > 0)
                    {
                        if (!elencoProfPrec.Contains(righeFile[i].Split('-')[0].Trim().ToUpper()))
                            elencoProfPrec.Add(righeFile[i].Split('-')[0].Trim().ToUpper());
                        elencoOre.Add(righeFile[i]);
                    }
                }
                allNotEnable();
            }
        }

        private void creaTabellaOrari()
        {
            /*
            //creazione della griglia base
            _3aInf_Matrice = new Attivita[giorni, ore];
            _4aInf_Matrice = new Attivita[giorni, ore];
            _5aInf_Matrice = new Attivita[giorni, ore];
            _3bInf_Matrice = new Attivita[giorni, ore];
            _4bInf_Matrice = new Attivita[giorni, ore];
            _5bInf_Matrice = new Attivita[giorni, ore];
            _3cInf_Matrice = new Attivita[giorni, ore];
            _4cInf_Matrice = new Attivita[giorni, ore];
            _5cInf_Matrice = new Attivita[giorni, ore];
            */
            for (int i = 0; i < giorni; i++)
            {

                for (int j = 0; j < ore; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        Label lVuota = new Label();
                        lVuota.Width = sizeW;
                        lVuota.Height = sizeH;
                        lVuota.Location = new Point(marginLeft, marginTop);
                        lVuota.BackColor = Color.BurlyWood;
                        lVuota.Text = "";

                        this.Controls.Add(lVuota);
                    }

                    Button b = new Button();
                    b.Name = "bt_" + i + "_" + j;
                    b.Width = sizeW;
                    b.Height = sizeH;
                    b.Text = "";
                    b.Location = new Point((i + 1) * sizeW + marginLeft, (j + 1) * sizeH + marginTop);
                    b.BackColor = Color.PaleGoldenrod;
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderColor = Color.WhiteSmoke;
                    b.FlatAppearance.BorderSize = 1;
                    b.MouseUp += new MouseEventHandler(cellChange);
                    b.Click += new System.EventHandler(cellClick);
                    b.Enabled = false;
                    this.Controls.Add(b);

                    if (i == 0)
                    {
                        Label l = new Label();
                        l.Name = "lbOra" + j;
                        l.Width = sizeW;
                        l.Height = sizeH;
                        l.Location = new Point(marginLeft, (j + 1) * sizeH + marginTop);
                        l.BackColor = Color.BurlyWood;
                        l.TextAlign = ContentAlignment.MiddleCenter;

                        if (j == 0) l.Text = "8:00";
                        else if (j == 1) l.Text = "9:00";
                        else if (j == 2) l.Text = "10:00";
                        else if (j == 3) l.Text = "11:00";
                        else if (j == 4) l.Text = "12:00";

                        this.Controls.Add(l);
                    }

                    if (j == 0)
                    {
                        Label lb = new Label();
                        lb.Name = "lbGiorno" + i;
                        lb.Width = sizeW;
                        lb.Height = sizeH;
                        lb.Location = new Point((i + 1) * sizeW + marginLeft, marginTop);
                        lb.BackColor = Color.BurlyWood;
                        lb.TextAlign = ContentAlignment.MiddleCenter;

                        if (i == 0) lb.Text = "LUNEDI'";
                        else if (i == 1) lb.Text = "MARTEDI'";
                        else if (i == 2) lb.Text = "MERCOLEDI'";
                        else if (i == 3) lb.Text = "GIOVEDI'";
                        else if (i == 4) lb.Text = "VENERDI'";
                        else if (i == 5) lb.Text = "SABATO";

                        this.Controls.Add(lb);
                    }
                }
            }
        }

        private void riempiListe()
        {            
            //lettura da file di testo
            string filePath = Path.GetFullPath("OreAttivita.txt");
            StreamReader reader = new StreamReader(filePath);

            for (int i = 0; i < elencoOre.Count; i++)
            {
                string[] strSplitted = reader.ReadLine().Split('-');
                string nomeDocente = strSplitted[0].Trim();
                string nomeMateria = strSplitted[2].Trim();
                string nomeClasse = strSplitted[3].Trim();
                int nOre = Convert.ToInt32(strSplitted[1].Trim());
                int iGiornoLibero = Convert.ToInt32(strSplitted[4].Trim());

                for (int x = 0; x < nOre; x++)
                {
                    int count = 0;

                    for (int j = 0; j < listListeClassi.Count; j++)
                    {
                        if (nomeClasse == listListeClassi.ElementAt(j).ElementAt(0).getClasse())
                        {
                            listListeClassi.ElementAt(j).Add(new Attivita(nomeDocente, nomeMateria, nomeClasse, iGiornoLibero));
                            count++;
                        }
                    }
                    if (count == 0)
                    {
                        listListeClassi.Add(new List<Attivita>());
                        listListeClassi.Last().Add(new Attivita(nomeDocente, nomeMateria, nomeClasse, iGiornoLibero));
                    }
                    else count = 0;
                }
            }           

            for (int i = 0; i < listListeClassi.Count; i++) 
                listMatrici.Add(new Attivita[giorni, ore]);

            for (int i = 0; i < listListeClassi.Count; i++)
                cbClasse.Items.Add(listListeClassi.ElementAt(i).ElementAt(0).getClasse().ToUpper());

            for (int i = 0; i < listListeClassi.Count; i++)
            {
                for (int j = 0; j < listListeClassi.ElementAt(i).Count; j++)
                {
                    if (!cbDocenti.Items.Contains(listListeClassi.ElementAt(i).ElementAt(j).getDocente().ToUpper()))
                        cbDocenti.Items.Add(listListeClassi.ElementAt(i).ElementAt(j).getDocente().ToUpper());
                }
            }
            int indexToStart = 0;

            if (result == DialogResult.No)
            {
                filePath = Path.GetFullPath("OrarioSalvato.txt");
                StreamReader reader2 = new StreamReader(filePath);
                int l = (reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None)).Length;
                reader2.Close();
                indexToStart = Convert.ToInt32(Math.Floor(Convert.ToDouble(l / (giorni * ore))));
            }
            reader.Close();

            for (int i = indexToStart; i < listListeClassi.Count; i++)
                generaOrario(listMatrici.ElementAt(i), listListeClassi.ElementAt(i));

            if (result == DialogResult.No) reloadOrario();

            salvaOrario();
            svuotaGriglia();
        }
        
        private void salvaOrario() {
            string filePath = Path.GetFullPath("OrarioSalvato.txt");
            FileStream fileStr = new FileStream(filePath, FileMode.Create);

            StreamWriter writer = new StreamWriter(fileStr);

            for (int x = 0; x < listMatrici.Count; x++) {
                for (int i = 0; i < giorni; i++) {
                    for (int j = 0; j < ore; j++) {
                        writer.Write(listMatrici.ElementAt(x)[i,j].getDocente() + " - " +
                            listMatrici.ElementAt(x)[i, j].getMateria() + " - " + listMatrici.ElementAt(x)[i, j].getClasse() + " - " +
                            listMatrici.ElementAt(x)[i, j].getGiornoFree() + " - " + x + Environment.NewLine);
                    }
                }
            }
            writer.Flush();
            writer.Close();
        }
        
        private void svuotaGriglia()
        {
            //assegnamento di una stringa vuota a tutti i button
            for (int i = 0; i < giorni; i++)
            {
                for (int j = 0; j < ore; j++)
                {
                    this.Controls["bt_" + i + "_" + j].Text = "";
                    this.Controls["bt_" + i + "_" + j].Enabled = false;
                }
            }
        }

        private void consultazione(Attivita[,] c)
        {
            //assegnamento di un'attività precedentemente generata al proprio button
            for (int i = 0; i < giorni; i++)
            {
                for (int j = 0; j < ore; j++)
                {
                    Attivita tmp = c[i, j];
                    this.Controls["bt_" + i + "_" + j].Text = tmp.ToString();
                    this.Controls["bt_" + i + "_" + j].Enabled = true;
                }
            }
        }

        private void cellClick(Object sender, EventArgs e)
        {
            //valore del button cliccato
            Button myButton = (Button)sender;
            //split del nome, da cui ricaviamo posizione del button corrente
            string[] arr = myButton.Name.Split('_');
            int riga = Convert.ToInt32(arr[1]);
            int colonna = Convert.ToInt32(arr[2]);

            if (lbNameClass.Text.Split(':')[0] == "CLASSE")
            {
                //Apparizione messagebox al click sul button di una classe
                for (int i = 0; i < listListeClassi.Count; i++) {
                    if(cbClasse.SelectedIndex == i) MessageBox.Show(listMatrici.ElementAt(i)[riga, colonna].showDetails());
                }
            }
            //Apparizione messagebox al click sul button di un docente
            string docente = lbNameClass.Text.Split(':')[1].Trim().ToLower();

            for (int i = 0; i < listMatrici.Count; i++)
            {
                if (listMatrici.ElementAt(i)[riga, colonna].getDocente().ToLower() == docente) MessageBox.Show(listMatrici.ElementAt(i)[riga, colonna].showDetails());
            }
            
        }

        private void scambioManuale()
        {
            //assegniamo a due variabili attività le due attività da scambiare
            Attivita cellaUno = listMatrici.ElementAt(cbClasse.SelectedIndex)[locButClicked[0], locButClicked[1]];
            Attivita cellaDue = listMatrici.ElementAt(cbClasse.SelectedIndex)[locButClicked[2], locButClicked[3]];

            //null delle attività selezionate
            listMatrici.ElementAt(cbClasse.SelectedIndex)[locButClicked[0], locButClicked[1]] = null;
            listMatrici.ElementAt(cbClasse.SelectedIndex)[locButClicked[2], locButClicked[3]] = null;

            //se rispetta la condizione ->scambio delle attività, altrimenti-> no scambio
            if (isAmmissibile(locButClicked[0], locButClicked[1], cellaDue, listMatrici.ElementAt(cbClasse.SelectedIndex), 0) && (isAmmissibile(locButClicked[2], locButClicked[3], cellaUno, listMatrici.ElementAt(cbClasse.SelectedIndex), 0)))
            {
                listMatrici.ElementAt(cbClasse.SelectedIndex)[locButClicked[0], locButClicked[1]] = cellaDue;
                listMatrici.ElementAt(cbClasse.SelectedIndex)[locButClicked[2], locButClicked[3]] = cellaUno;

                this.Controls["bt_" + locButClicked[0] + "_" + locButClicked[1]].Text = listMatrici.ElementAt(cbClasse.SelectedIndex)[locButClicked[0], locButClicked[1]].ToString();
                this.Controls["bt_" + locButClicked[2] + "_" + locButClicked[3]].Text = listMatrici.ElementAt(cbClasse.SelectedIndex)[locButClicked[2], locButClicked[3]].ToString();
            }
            else
            {
                listMatrici.ElementAt(cbClasse.SelectedIndex)[locButClicked[0], locButClicked[1]] = cellaUno;
                listMatrici.ElementAt(cbClasse.SelectedIndex)[locButClicked[2], locButClicked[3]] = cellaDue;
            }
        }

        private void cbClasse_SelectedIndexChanged(object sender, EventArgs e)
        {
            //apparizione orario classe
            for (int i = 0; i < listMatrici.Count; i++) {
                if (cbClasse.SelectedIndex == i) {
                    consultazione(listMatrici.ElementAt(i));
                    lbNameClass.Text = "CLASSE: " + listListeClassi.ElementAt(i).ElementAt(0).getClasse();
                }
            }
        }

        private void cellChange(object sender, MouseEventArgs e)
        {
            //controllo che lo scambio stia avvenendo in ambito interno ad una classe
            if (lbNameClass.Text.Split(':')[0] == "CLASSE")
            {
                Button myButton = (Button)sender;

                //assegnamento indice ad una determinata matrice
                if (e.Button == MouseButtons.Right)
                {
                    /*
                    int indexer = -1;

                    if (matriceCaricata == listMatrici.ElementAt(0)) indexer = 0;
                    else if (matriceCaricata == listMatrici.ElementAt(1)) indexer = 1;
                    else if (matriceCaricata == _3cInf_Matrice) indexer = 2;
                    else if (matriceCaricata == _4aInf_Matrice) indexer = 3;
                    else if (matriceCaricata == _4bInf_Matrice) indexer = 4;
                    else if (matriceCaricata == _4cInf_Matrice) indexer = 5;
                    else if (matriceCaricata == _5aInf_Matrice) indexer = 6;
                    else if (matriceCaricata == _5bInf_Matrice) indexer = 7;
                    else if (matriceCaricata == _5cInf_Matrice) indexer = 8;
                    */
                    //grafica button
                    if (myButton.BackColor != Color.BurlyWood)
                    {
                        nClick++;
                        myButton.BackColor = Color.BurlyWood;
                    }
                    else
                    {
                        nClick--;
                        myButton.BackColor = Color.PaleGoldenrod;
                    }
                    //posizione prima attività
                    if (nClick == 1)
                    {
                        locButClicked[0] = Convert.ToInt32(myButton.Name.Split('_')[1]);
                        locButClicked[1] = Convert.ToInt32(myButton.Name.Split('_')[2]);
                    }
                    //posizione seconda attività
                    if (nClick == 2)
                    {
                        locButClicked[2] = Convert.ToInt32(myButton.Name.Split('_')[1]);
                        locButClicked[3] = Convert.ToInt32(myButton.Name.Split('_')[2]);
                        nClick = 0;

                        this.Controls["bt_" + locButClicked[0] + "_" + locButClicked[1]].BackColor = Color.PaleGoldenrod;
                        this.Controls["bt_" + locButClicked[2] + "_" + locButClicked[3]].BackColor = Color.PaleGoldenrod;

                        //scambio
                        scambioManuale();
                        /*
                        switch (indexer)
                        {
                            case 0:
                                listMatrici.ElementAt(0) = matriceCaricata;
                                break;
                            case 1:
                                listMatrici.ElementAt(1) = matriceCaricata;
                                break;
                            case 2:
                                _3cInf_Matrice = matriceCaricata;
                                break;
                            case 3:
                                _4aInf_Matrice = matriceCaricata;
                                break;
                            case 4:
                                _4bInf_Matrice = matriceCaricata;
                                break;
                            case 5:
                                _4cInf_Matrice = matriceCaricata;
                                break;
                            case 6:
                                _5aInf_Matrice = matriceCaricata;
                                break;
                            case 7:
                                _5bInf_Matrice = matriceCaricata;
                                break;
                            case 8:
                                _5cInf_Matrice = matriceCaricata;
                                break;
                        }
                        */
                    }
                }
            }
        }

        private void generaOrario(Attivita[,] c, List<Attivita> list)
        {
            //generazione orario di una classe
            List<Attivita> l = new List<Attivita>(list);
            Random r = new Random();
            int[] indexs = new int[2];

            for (int i = 0; i < giorni; i++)
            {
                for (int j = 0; j < ore; j++)
                {
                    if (l.Count > 0)
                    {
                        //prelevamento attività dalla lista delle attività di quella determinata classe 
                        int n = r.Next(l.Count());
                        Attivita tmp = l.ElementAt(n);
                        l.RemoveAt(n);

                        //se è ammissibile assegna attività alla cella di una mtrice
                        if (isAmmissibile(i, j, tmp, c, 1))
                        {
                            c[i, j] = tmp;
                            this.Controls["bt_" + i + "_" + j].Text = tmp.ToString();
                        }

                        //se non è ammissibile, l'attività viene inserita in attività sospese
                        else
                        {
                            _attivitaSospese.Add(tmp);
                            this.Controls["bt_" + i + "_" + j].Text = "";
                        }

                        //inserimento di 2 ore di una delle seguenti materie
                        if (tmp.getMateria() == "Informatica")
                        {
                            indexs = doppieOre(tmp, "Informatica", l, c, i, j);
                            i = indexs[0];
                            j = indexs[1];
                        }
                        else if (tmp.getMateria() == "Lettere")
                        {
                            indexs = doppieOre(tmp, "Lettere", l, c, i, j);
                            i = indexs[0];
                            j = indexs[1];
                        }
                        else if (tmp.getMateria() == "Ginnastica")
                        {
                            indexs = doppieOre(tmp, "Ginnastica", l, c, i, j);
                            i = indexs[0];
                            j = indexs[1];
                        }
                    }
                }
            }
            //assegnamento attività sospese alla matrice
            if (_attivitaSospese.Count > 0) scambiOre(c);
        }

        private bool oreSoloVicine(int i, int j, Attivita[,] c, string doc, int a)
        {
            int countMateria = 0;
            int count = 0;
            //controllo se l'attività di una materia è consecutiva ad un'altra della stessa materia
            for (int x = 0; x < ore; x++)
            {
                if (c[i, x] != null && c[i, x].getDocente() == doc) countMateria++;
            }

            if (j - 1 >= 0 && j + 1 < ore)
            {
                if ((c[i, j - 1] == null || c[i, j - 1].getDocente() != doc) && (c[i, j + 1] == null || c[i, j + 1].getDocente() != doc) && countMateria > 0) count++;
            }
            else if (j - 1 >= 0)
            {
                if ((c[i, j - 1] == null || c[i, j - 1].getDocente() != doc) && countMateria > 0) count++;
            }
            else if (j + 1 < ore)
            {
                if ((c[i, j + 1] == null || c[i, j + 1].getDocente() != doc) && countMateria > 0) count++;
            }

            //impossibile inserire più di 2 ore di fila di una materia in una giornata
            if (countMateria > 1)
            {
                if (a == 0) MessageBox.Show("Già presenti 2 ore di " + doc + " nella stessa giornata");
                return false;
            }

            //controllo inse un'attività di una materia non consecutiva ad un'altra della stessa materia
            if (count > 0)
            {
                if (a == 0) MessageBox.Show("Ora di " + doc + " già presente nella giornata e non vicina.");
                return false;
            }
            else return true;
        }

        private bool isAmmissibile(int i, int j, Attivita att, Attivita[,] c, int a)
        {
            int x = 1;
            if (a == 0) x = 0;

            //imposto giorno libero al docente
            bool ggLiberi = i == att.getGiornoFree();

            //controllo inserimento di un'attività nel giorno libero del prof
            if (ggLiberi && a == 0) MessageBox.Show("Non ammissibile! Questo è il giorno libero di " + att.getDocente());
            if (ggLiberi) return false;

            //controllo per ogni classe se è possibile inserire l'attività in quella precisa ora
            bool vincolo = listMatrici.All(m => m[i, j] == null || m[i, j].getDocente() != att.getDocente());
            if (vincolo)
                return oreSoloVicine(i, j, c, att.getDocente(), x);

            if (a == 0) MessageBox.Show("Non ammissibile! Il professore " + att.getDocente() + " è già occupato.");
            return false;
        }

        private int[] doppieOre(Attivita tmp, string m, List<Attivita> l, Attivita[,] c, int i, int j)
        {
            int index = -1;
            int count = 0;

            for (int z = 0; z < l.Count; z++) if (l.ElementAt(z).getMateria() == m) count++;

            //per informatica, italiano e ginnastica, se è possibile inseriamo due ore di fila
            if (count > 0)
            {
                foreach (Attivita x in l) if (x.getMateria() == m) index = l.IndexOf(x);

                l.RemoveAt(index);

                if (j + 1 == ore) { j = 0; i++; }
                else j++;

                if (isAmmissibile(i, j, tmp, c, 1))
                {
                    c[i, j] = tmp;
                    this.Controls["bt_" + i + "_" + j].Text = tmp.ToString();
                }
                else
                {
                    _attivitaSospese.Add(tmp);
                    this.Controls["bt_" + i + "_" + j].Text = "";
                }
            }
            int[] indexsToReturn = { i, j };

            return indexsToReturn;
        }

        private void scambiOre(Attivita[,] c)
        {
            //salvataggio coordinate di un button
            List<int> indexs = new List<int>();
            int countTent = 0;
            for (int i = 0; i < giorni; i++)
            {
                for (int j = 0; j < ore; j++)
                {
                    if (c[i, j] == null) { indexs.Add(i); indexs.Add(j); }
                }
            }

            //scambio tra due ore
            while (_attivitaSospese.Count > 0)
            {
                for (int i = 0; i < giorni; i++)
                {
                    for (int j = 0; j < ore; j++)
                    {
                        if (_attivitaSospese.Count > 0)
                        {
                            countTent++;
                            //condizione anti-loop -> se entro 30 tentativi ancora deve piazzare delle attività-> RemoveCasualCell
                            if (countTent == tentativi)
                            {
                                removeCasualCell(c, indexs);
                                countTent = 0;
                            }

                            //scambio attività con una cella null
                            if (c[i, j] == null && isAmmissibile(i, j, _attivitaSospese.ElementAt(0), c, 1))
                            {

                                c[i, j] = _attivitaSospese.ElementAt(0);
                                this.Controls["bt_" + i + "_" + j].Text = c[i, j].ToString();
                                _attivitaSospese.RemoveAt(0);

                                int dim = indexs.Count;

                                for (int x = 0; x < dim; x++) indexs.RemoveAt(0);

                                for (int z = 0; z < giorni; z++)
                                {
                                    for (int k = 0; k < ore; k++)
                                    {
                                        if (c[z, k] == null) { indexs.Add(z); indexs.Add(k); }
                                    }
                                }
                            }

                            //scambio tra due attività
                            else if (c[i, j] != null && _attivitaSospese.ElementAt(0).getDocente() != c[i, j].getDocente())
                            {
                                if (isAmmissibile(i, j, _attivitaSospese.ElementAt(0), c, 1) && isAmmissibile(indexs.ElementAt(0), indexs.ElementAt(1), c[i, j], c, 1))
                                {
                                    Attivita tmp = c[i, j];
                                    c[i, j] = _attivitaSospese.ElementAt(0);
                                    c[indexs.ElementAt(0), indexs.ElementAt(1)] = tmp;

                                    this.Controls["bt_" + i + "_" + j].Text = c[i, j].ToString();

                                    this.Controls["bt_" + indexs.ElementAt(0) + "_" + indexs.ElementAt(1)].Text = tmp.ToString();

                                    indexs.RemoveAt(0);
                                    indexs.RemoveAt(0);
                                    _attivitaSospese.RemoveAt(0);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void removeCasualCell(Attivita[,] c, List<int> indexs)
        {
            //prelevo cella in maniera random
            int i = listMatrici.IndexOf(c);
            Random r = new Random();
            int gg = r.Next(giorni);
            int ora = r.Next(ore);


            if (c[gg, ora] != null)
            {
                //aggiunta dell'attività in _attivitaSospese, cella diventa null
                _attivitaSospese.Add(c[gg, ora]);
                c[gg, ora] = null;

                //aggiorniamento matrice corrente
                Attivita[][,] arr = listMatrici.ToArray();
                arr[i] = c;
                listMatrici = arr.ToList();

                //aggiunta ad indexs degli indici della nua cella null
                indexs.Add(gg);
                indexs.Add(ora);
            }

            // mischia casualmente attività sospese
            _attivitaSospese = _attivitaSospese.OrderBy(a => Guid.NewGuid()).ToList();
        }

        private void cbDocenti_SelectedIndexChanged(object sender, EventArgs e)
        {
            svuotaGriglia();
            //visualizza orario del singolo prof
            string doc = cbDocenti.Text;
            lbNameClass.Text = "DOCENTE: " + doc.ToUpper();
            for (int x = 0; x < listMatrici.Count; x++)
            {
                for (int i = 0; i < giorni; i++)
                {
                    for (int j = 0; j < ore; j++)
                    {
                        if (listMatrici.ElementAt(x)[i, j].getDocente().ToUpper() == doc.ToUpper())
                        {
                            this.Controls["bt_" + i + "_" + j].Enabled = true;
                            this.Controls["bt_" + i + "_" + j].Text = listMatrici.ElementAt(x)[i, j].getDocente().ToUpper();
                        }
                    }
                }
            }
        }

        private void btAddClasse_Click(object sender, EventArgs e)
        {
            btAddClasse.Enabled = false;
            btNomeClasse.Enabled = true;
            tbNomeClasse.Enabled = true;
            btRemoveLastClass.Enabled = false;
            btEnd.Enabled = false;
        }

        private void btNomeClasse_Click(object sender, EventArgs e)
        {
            int count = 0;
            foreach (string ora in elencoOre.ToList()) {
                if (ora.Split('-')[3].Trim().ToLower() == tbNomeClasse.Text.ToLower())                
                    count++;
            }
            if (tbNomeClasse.Text != "") {
                if (count == 0)
                {
                    btNomeClasse.Enabled = false;
                    tbNomeClasse.Enabled = false;
                    tbNomeProfessore.Enabled = true;
                    tbNomeMateria.Enabled = true;
                    tbNumOre.Enabled = true;
                    btAddOre.Enabled = true;
                    btRemove.Enabled = true;
                    lbLastClass.Text = "Ultima classe:  " + tbNomeClasse.Text;
                    lbEndClass.Text = tbNomeClasse.Text;
                }
                else MessageBox.Show("Hai già creato una classe con lo stesso nome!");
            }
            else MessageBox.Show("Devi dare un nome alla classe!");
        }

        int oreRimaste = 30;
        private void btAddOre_Click(object sender, EventArgs e)
        {
            string nProf = tbNomeProfessore.Text;
            int nOre = 0;
            try {
                if (Convert.ToInt32(tbNumOre.Text) <= 6)
                    nOre = Convert.ToInt32(tbNumOre.Text);
                else MessageBox.Show("Puoi massimo inserire 6 ore per volta!!");
            }
            catch {
                MessageBox.Show("Digitare un numero!!");
            }
            string nMateria = tbNomeMateria.Text;
            string nClasse = tbNomeClasse.Text;

            int countNumOre = 0;
            int countNumOreMateria = 0;
            foreach (string ora in elencoOre.ToList()) {
                if (ora.Split('-')[3].Trim() == nClasse && ora.Split('-')[0].Trim() == nProf)
                {
                    countNumOre += Convert.ToInt32(ora.Split('-')[1].Trim());
                }
                if (ora.Split('-')[3].Trim() == nClasse && ora.Split('-')[2].Trim() == nMateria)
                {
                    countNumOreMateria += Convert.ToInt32(ora.Split('-')[1].Trim());
                }
            }
            if (countNumOre + nOre > 6) MessageBox.Show("Un docente non può fare più di 6 ore per classe!");
            else if (countNumOreMateria + nOre > 6) MessageBox.Show("Ci possono essere massimo 6 ore di una materia per classe!");
            else {
                if (nOre != 0) elencoOre.Add(nProf + " - " + nOre + " - " + nMateria + " - " + nClasse);
                if (oreRimaste - nOre >= 0)
                {
                    oreRimaste -= nOre;
                    lbOreRimaste.Text = "Ore rimaste:  " + oreRimaste;
                    if (elencoOre.Count > 0) lbLastHour.Text = "Ultime ore:  " + elencoOre.Last();
                }
                else MessageBox.Show("Stai aggiungendo troppe ore! Max 30!!!");
                if (oreRimaste == 0)
                {
                    tbNomeProfessore.Enabled = false;
                    tbNumOre.Enabled = false;
                    tbNomeMateria.Enabled = false;
                    btAddOre.Enabled = false;
                    btRemove.Enabled = false;
                    btRemoveLastClass.Enabled = true;
                    lbOreRimaste.Text = "Ore rimaste: ";
                    lbLastHour.Text = "Ultima ore: ";
                    MessageBox.Show("Hai completato la classe: " + nClasse);
                    tbNomeProfessore.Text = "";
                    tbNumOre.Text = "";
                    tbNomeMateria.Text = "";
                    tbNomeClasse.Text = "";
                    btAddClasse.Enabled = true;
                    btEnd.Enabled = true;
                    oreRimaste = 30;
                }
            }
        }

        private void btRemove_Click(object sender, EventArgs e)
        {
            try {
                int ora = Convert.ToInt32(elencoOre.Last().Split('-')[1]);
                oreRimaste += ora;
                lbOreRimaste.Text = "Ore rimaste: " + oreRimaste;                
                elencoOre.RemoveAt(elencoOre.Count - 1);
                if (elencoOre.Count == 0) lbLastHour.Text = "Ultima ore: ";
                else lbLastHour.Text = "Ultima ore: " + elencoOre.Last();
            }
            catch {
                MessageBox.Show("Non ci sono più ore da rimuovere!");
            }
        }

        private void btRemoveLastClass_Click(object sender, EventArgs e)
        {
            if (elencoOre.Count > 0)
            {
                string classe = lbEndClass.Text;
                foreach (string ora in elencoOre.ToList())
                {
                    if (ora.Split('-')[3].Trim() == classe)
                    {
                        elencoOre.Remove(ora);
                    }
                }
                if (elencoOre.Count > 0)
                {
                    string lastClass = elencoOre.Last().Split('-')[3].Trim();
                    lbLastClass.Text = "Ultima classe:  " + lastClass;
                    lbEndClass.Text = lastClass;
                }
                else lbLastClass.Text = "Ultima classe:  ";
            }
            else
            {
                MessageBox.Show("Non ci sono classi da rimuovere");
                lbLastClass.Text = "Ultima classe:  ";
            }            
        }

        private void btEnd_Click(object sender, EventArgs e)
        {
            if (elencoOre.Count > 0)
            {
                btAddClasse.Enabled = false;
                btRemoveLastClass.Enabled = false;
                btEnd.Enabled = false;
                cbGiorniFree.Enabled = true;
                btGiorniFree.Enabled = true;

                string filePath = Path.GetFullPath("OreAttivita.txt");
                StreamReader reader = new StreamReader(filePath);
                string[] righeFile = reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None);
                reader.Close();

                foreach (string ora in elencoOre.ToList()) {
                    if (!elencoProfPrec.Contains(ora.Split('-')[0].Trim().ToUpper()))
                        elencoProf.Add(ora.Split('-')[0].Trim().ToUpper());
                    else if (ora.Split('-').Length < 5)
                    {
                        string oraToSub = ora;
                        elencoOre.Remove(ora);

                        foreach (string prof in elencoOre.ToList()) {
                            if (oraToSub.Split('-')[0].Trim().ToUpper() == prof.Split('-')[0].Trim().ToUpper())
                                oraToSub += " - " + Convert.ToInt32(prof.Split('-')[4].Trim());
                        }
                        elencoOre.Add(oraToSub);
                    }
                }
                if (elencoProf.Count > 0) {
                    btGiorniFree.Text = "GIORNO LIBERO: " + elencoProf.Last();
                    lbProf.Text = elencoProf.Last();
                }
                else {
                    btGiorniFree.Text = "CLICCA PER L'ORARIO";
                    cbGiorniFree.Enabled = false;
                    lbProf.Text = "";
                }
            }
            else MessageBox.Show("Non hai creato nessuna classe!!!");
        }

        private void btGiorniFree_Click(object sender, EventArgs e)
        {
            string prof = lbProf.Text;
            int iGiornoLibero = -1;
            switch (cbGiorniFree.SelectedIndex)
            {
                case 0:
                    iGiornoLibero = 0;
                    break;
                case 1:
                    iGiornoLibero = 1;
                    break;
                case 2:
                    iGiornoLibero = 2;
                    break;
                case 3:
                    iGiornoLibero = 3;
                    break;
                case 4:
                    iGiornoLibero = 4;
                    break;
                case 5:
                    iGiornoLibero = 5;
                    break;
                default:
                    iGiornoLibero = -1;
                    if (cbGiorniFree.Enabled == true) MessageBox.Show("Seleziona un giorno!");
                    break;
            }
            if (iGiornoLibero != -1 == cbGiorniFree.Enabled == true)
            {
                string nwElementList;
                foreach (string ora in elencoOre.ToList())
                {
                    if (ora.Split('-')[0].Trim().ToUpper() == prof) {
                        nwElementList = ora + " - " + iGiornoLibero;
                        elencoOre.Remove(ora);
                        elencoOre.Add(nwElementList);
                    }
                }
                if (elencoProf.Count > 0)
                {
                    elencoProf.RemoveAt(elencoProf.Count - 1);
                    if (elencoProf.Count > 0) {
                        lbProf.Text = elencoProf.Last();
                        btGiorniFree.Text = "GIORNO LIBERO: " + elencoProf.Last();
                    }
                    else {
                        allInvisible();
                        cbClasse.Visible = true;
                        cbDocenti.Visible = true;
                        stampaDati();                        
                        creaTabellaOrari();
                        riempiListe();
                    }
                }
                else
                {
                    allInvisible();
                    cbClasse.Visible = true;
                    cbDocenti.Visible = true;
                    stampaDati();                    
                    creaTabellaOrari();
                    riempiListe();
                }
            }
        }

        private void reloadOrario()
        {
            string filePath = Path.GetFullPath("OrarioSalvato.txt");
            StreamReader reader = new StreamReader(filePath);

            int l = (reader.ReadToEnd().Split(new string[] { "\r\n" }, StringSplitOptions.None)).Length;
            int nMatrix = Convert.ToInt32(Math.Floor(Convert.ToDouble(l / (giorni * ore))));
            reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

            for (int i = 0; i < nMatrix; i++)
            {
                for(int j = 0; j < giorni; j++)
                {
                    for (int x = 0; x < ore; x++)
                    {
                        string riga = reader.ReadLine();
                        string nDoc = riga.Split('-')[0].Trim();
                        string materia = riga.Split('-')[1].Trim();
                        string classe = riga.Split('-')[2].Trim();
                        int gFree = Convert.ToInt32(riga.Split('-')[3].Trim());
                        listMatrici.ElementAt(i)[j, x] = new Attivita(nDoc, materia, classe, gFree);
                    }
                }                
            }
            reader.Close();
        }

        private void stampaDati() {
            string filePath = Path.GetFullPath("OreAttivita.txt");
            FileStream fileStr = new FileStream(filePath, FileMode.Create);
            StreamWriter writer = new StreamWriter(fileStr);
            foreach (string ora in elencoOre.ToList())
                writer.Write(ora + Environment.NewLine);
            writer.Flush();
            writer.Close();            
        }

        private void allInvisible() {
            btGiorniFree.Visible = false;
            cbGiorniFree.Visible = false;
            tbNomeClasse.Visible = false;
            tbNomeMateria.Visible = false;
            tbNomeProfessore.Visible = false;
            tbNumOre.Visible = false;
            btAddClasse.Visible = false;
            btAddOre.Visible = false;
            btEnd.Visible = false;
            btRemove.Visible = false;
            btRemoveLastClass.Visible = false;
            btNomeClasse.Visible = false;
            lbLastClass.Visible = false;
            lbLastHour.Visible = false;
            lbNameClass.Visible = false;
            lbOreRimaste.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
        }

        private void allNotEnable()
        {
            btNomeClasse.Enabled = false;
            tbNomeClasse.Enabled = false;
            btAddOre.Enabled = false;
            tbNomeMateria.Enabled = false;
            tbNomeProfessore.Enabled = false;
            tbNumOre.Enabled = false;
            btRemove.Enabled = false;
            btRemoveLastClass.Enabled = false;
            cbGiorniFree.Enabled = false;
            btGiorniFree.Enabled = false;
            cbClasse.Visible = false;
            cbDocenti.Visible = false;
        }

        private void tbNomeProfessore_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
