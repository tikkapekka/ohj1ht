using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

public class HT
{
    /// <summary>
    /// Etsitään keikkakalenterista Jyväskylän keikat.
    /// Jos Jyväskylässä ei ole keikkoja, näytetään koko maan keikat.
    /// Aakkosjärjestyksen voi valita artistin tai paikkakunnan mukaan.
    /// </summary>
    public static void Main(string[] args)

    {
        string url = @"https://www.keikkakunkku.fi/keikat/paivan-keikat/";
        string sivu = LueNetista(url);

        int alku = sivu.IndexOf("startDate") - 100;
        int loppu = sivu.IndexOf("Pidätämme");

        string[] rivit = ErotaListaus(sivu, alku, loppu);

        //int[] t = {2, 3, 4};

        List<string> taso2 = KeraaTaso(rivit, 2);
        List<string> taso3 = KeraaTaso(rivit, 3);
        List<string> taso4 = KeraaTaso(rivit, 4);

        //List<string> kaikki = KeraaTasot(rivit, t);
        //Tulosta(kaikki);


        List<string> artistit     = Etsi(taso2, "name");
        List<string> keikkapaikat = Etsi(taso3, "name");
        List<string> paikkakunnat = Etsi(taso4, "addressLocality");

        keikkapaikat = PoistaTuplat(keikkapaikat);
        int montako = keikkapaikat.Count;

        List<List<string>> listat = new() { artistit, keikkapaikat, paikkakunnat };

        listat = Siivoa(listat);

        List<string> yhteislista = ListatSamaan(listat, montako);
        
        Console.WriteLine("Tämän päivän keikat:");
        Console.WriteLine("--------------------------------------------------------------------------------");
        TulostaValein(yhteislista, 3);
        ////yhteislista = VaihdaMerkit(yhteislista);
    }


    /// <summary>
    /// Hakee URL-osoitteen perusteella HTML-sivun sisällön.
    /// </summary>
    /// <param name="url">URL-osoite, josta haetaan.</param>
    /// <returns>Verkkosivun sisältö.</returns>
    public static string LueNetista(string url)
    {
        var client = new HttpClient();
        string content = client.GetStringAsync(url).GetAwaiter().GetResult();
        return content;
    }


    /// <summary>
    /// Lisää pätkän merkkijonosta tiedostopolkuun riveittäin.
    /// </summary>
    /// <param name="jono">Käsiteltävä merkkijono.</param>
    /// <param name="alku">Indeksi josta uusi merkkijono alkaa.</param>
    /// <param name="loppu">Indeksi johon uusi merkkijono päättyy.</param>
    /// <returns>Tekstitiedoston rivit merkkijonotaulukkona.</returns>
    public static string[] ErotaListaus(string jono, int alku, int loppu)
    {
        StringBuilder sb = new();
        sb.Append(jono);
        string lyhempi = sb.ToString(alku, loppu - alku);
        string[] rivit = lyhempi.Split('\n');
        return rivit;
    }
    

    //public static List<string> VaihdaMerkit(List<string> lista)
    //{
    //    List<string> lista2 = new();
    //    for (int i = 0; (i < lista.Count-1); i++)
    //    {
    //        string muutettu = lista[i].Replace("Vii", "ZZZ");
    //        lista2[i] = muutettu;
    //    }
    //    return lista2;
    //}


    /// <summary>
    /// Kerätään listaan tietyn verran sisennetyt rivit, jotta
    /// hakusanoja voidaan käyttää. 1 taso = 4 välilyöntiä.
    /// </summary>
    /// <param name = "rivit" > Käsiteltävät rivit</param>
    /// <param name = "taso" > Sisennyksen taso.</param>
    /// <returns>Lista johon kerätään.</returns>
    public static List<string> KeraaTaso(string[] rivit, int taso)  // kuormitettuna poista ylimääräiset
    {
        List<string> rivit2 = new();
        for (int i = 0; i < rivit.Length; i++)
        {
            if (rivit[i].IndexOf('"') == taso * 4) rivit2.Add(rivit[i]);
        }
        return rivit2;
    }


    //public static List<string> KeraaTasot(string[] rivit, int[] tasot)
    //{
    //    List<string> rivit2 = new();
    //    for (int i = 0; i < rivit.Length; i++)
    //        foreach (int taso in tasot)
    //        {
    //            if (rivit[i].IndexOf('"') == taso * 4) rivit2.Add(rivit[i]);
    //        }
    //    return rivit2;
    //}


    /// <summary>
    /// Lisää hakusanan sisältävät rivit listaan.
    /// </summary>
    /// <param name="rivit">Käsiteltävät rivit.</param>
    /// <param name="haettava">Hakusana.</param>
    /// <returns>Lista riveistä jotka sisältävät hakusanan.</returns>
    public static List<string> Etsi(List<string> rivit, string haettava)
    {
        List<string> lista = new();
        foreach (string jono in rivit)
        {
            if (jono.Contains(haettava)) lista.Add(jono);
        }
        return lista;
    }


    public static List<List<string>> Siivoa(List<List<string>> listat)
    {
        foreach(List <string> lista in listat)
        {
            Siivoa(lista);
        }
        return listat;
    }


    /// <summary>
    /// Poistaa rivien alusta ja lopusta tyhjän tilan ja tunnisteet.
    /// </summary>
    /// <param name="lista">Käsiteltävä lista.</param>
    /// <returns>Tarvittava tieto.</returns>
    public static List<string> Siivoa(List<string> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        { 
            int paikka = lista[i].IndexOf(':') + 3;
            int pituus = lista[i].Length;
            if (!lista[i][^1].Equals(',')) pituus++;
            
            StringBuilder sb = new();
            sb = sb.Append(lista[i]);
            lista[i] = sb.ToString(paikka, (pituus - 2) - paikka);
        }
        return lista;
    }


    /// <summary>
    /// Poistaa listasta parilliset alkiot.
    /// </summary>
    /// <param name="lista">Käsiteltävä lista.</param>
    /// <returns>Uusi lista.</returns>
    public static List<string> PoistaTuplat(List<string> lista)
    {
        List<string> lista2 = new();
        for (int i = 1; i < lista.Count; i += 2) lista2.Add(lista[i]);
        return lista2;
    }


    /// <summary>
    /// Tulostaa listassa olevat listat.
    /// </summary>
    /// <param name="listat"></param>
    public static void Tulosta(List<List<string>> listat)
    {
        foreach (List<string> lista in listat)
        {
            Tulosta(lista);
            Console.WriteLine();
            Console.Write("------------------------------------------------------------------------------------------");
        }
    }


    /// <summary>
    /// Tulostaa kolme perättäistä alkiota.
    /// </summary>
    /// <param name="rivit">Tulostettava lista.</param>
    /// <param name="vali"></param>
    public static void TulostaValein(List<string> rivit, int vali)  /// tää kivasti parametreina pitäis mennä kuormitettuna
    {
      for (int i = 0; i < rivit.Count; i += vali)
        {
            Console.WriteLine(String.Format("{0,-30} {1,-40} {2,-30}", rivit[i], rivit[i + 1], rivit[i + 2]));
        }
    }


    /// <summary>
    /// Tulostaa annetun listan rivi kerrallaan.
    /// </summary>
    /// <param name="rivit">Lista.</param>
    public static void Tulosta(List<string> rivit)
    {
        foreach (string rivi in rivit) Console.WriteLine(rivi); 
    }


    /// <summary>
    /// Muodostaa listoista uuden listan.
    /// Listojen tulee olla yhtä pitkiä.
    /// </summary>
    /// <param name="listat">Käsiteltävät listat.</param>
    /// <param name="montako">Montako alkiota listoissa on.</param>
    /// <returns></returns>
    public static List<string> ListatSamaan(List<List<string>> listat, int montako)
    {
        List<string> listat2 = new();
        for (int i = 0; i < montako; i++)
            foreach (List<string> lista in listat) listat2.Add(lista[i]); //tähän jotain poikkeuksia pitää miettiä
        return listat2;
    }


    //public static string HaeIndeksit(string lista, string haettava)
    //{
    //    StringBuilder taulu = new();
    //    foreach (string jono in lista)
    //    {
    //        if (jono.Contains(haettava)) taulu.Append(", ").Append(jono);
    //    }
    //    return taulu.ToString();
    //}

}