using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Text.Encodings.Web;
using System.Security.AccessControl;
using System.Text.Json.Serialization.Metadata;
using System.Globalization;
using System.Net;

public class HT
{
    /// <summary>
    /// Näytetään keikkakalenterista tämän päivän keikat.
    /// </summary>
    public static void Main()

    {
        string url = @"https://www.keikkakunkku.fi/keikat/kaikki-keikat/";
        string html = LueNetista(url);


        int[] t = {2, 3, 4};

        List<List<string>> kaikki = KeraaTasot(html, t);


        List<string> artistit = Etsi(kaikki[0], "name");
        List<string> pvm = Etsi(kaikki[0], "startDate");
        List<string> keikkapaikat = Etsi(kaikki[1], "name");
        List<string> paikkakunnat = Etsi(kaikki[2], "addressLocality");
        keikkapaikat = PoistaTuplat(keikkapaikat);
        List<DateTime> paivat = EtsiPvm(pvm);
        //Tulosta(artistit);
        //TulostaAjat(paivat);
        //Tulosta(keikkapaikat);
        //Tulosta(paikkakunnat);
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
    /// Lisää pätkän merkkijonosta riveittäin taulukkoon.
    /// </summary>
    /// <param name = "jono" > Käsiteltävä merkkijono.</param>
    /// <param name = "alku" > Indeksi josta uusi merkkijono alkaa.</param>
    /// <param name = "loppu" > Indeksi johon uusi merkkijono päättyy.</param>
    /// <returns>Tekstitiedoston rivit merkkijonotaulukkona.</returns>
    public static string[] ErotaListaus(string jono)
    {
        int alku = jono.IndexOf("startDate") - 100;
        int loppu = jono.LastIndexOf("addressCountry");
        int pituus = loppu - alku;
        string koko = jono.Substring(alku, pituus);
        string[] rivit = koko.Split('\n');
        return rivit;
    }

    /// <summary>
    /// Kerää kerralla taulukkona annetut indentaatiotasot.
    /// </summary>
    /// <param name="html">Käsiteltävä merkkijono.</param>
    /// <param name="t">Taulukko indentaatiotasoista.</param>
    /// <returns>Tasot listoina.</returns>
    public static List<List<string>> KeraaTasot(string html, int[] t)
    {
        string[] rivit = ErotaListaus(html);
        List<List<string>> tasot = new();
        foreach (int luku in t)
        {
            List<string> taso = KeraaTaso(html, luku);
            tasot.Add(taso);
        }
        return tasot;
    }
    

    /// <summary>
    /// Kerätään listaan rivit joilla on annettu indentaatiotaso.
    /// 1 taso = 4 välilyöntiä.
    /// </summary>
    /// <param name="rivit"> Käsiteltävät rivit</param>
    /// <param name="taso"> Sisennyksen taso.</param>
    /// <returns>Lista johon kerätään.</returns>
    public static List<string> KeraaTaso(string html, int taso)
    {
        string[] rivit = ErotaListaus(html);
        List<string> rivit2 = new();
        foreach (string rivi in rivit)
        {
            if (rivi.IndexOf('"') == taso * 4) rivit2.Add(rivi);
        }
        return rivit2;
    }


    /// <summary>
    /// Lisää hakusanan sisältävät rivit uuteen listaan.
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
        Siivoa(lista);
        return lista;
    }

    /// <summary>
    /// Etsii tekstistä päivämäärät DateTime-objekteina.
    /// </summary>
    /// <param name="rivit">Käsitelvävät rivit</param>
    /// <returns>Lista päivämääristä.</returns>
    public static List<DateTime> EtsiPvm(List<string> rivit)
    {
        List<DateTime> lista = new();
        DateTime paiva = new DateTime();
        foreach (string jono in rivit)
        {
            {
                if (DateTime.TryParse(jono, out paiva)) lista.Add(paiva);
            }
        }
        return lista;
    }


    /// <summary>
    /// Siivoaa kaikki listan listat kerralla.
    /// </summary>
    /// <param name="listat">Käsiteltävä listojen lista.</param>
    /// <returns>Listat siivottuna.</returns>
    public static List<List<string>> Siivoa(List<List<string>> listat)
    {
        foreach (List<string> lista in listat) Siivoa(lista);
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
            StringBuilder sb = new();
            sb = sb.Append(lista[i]);
            int alku = lista[i].IndexOf(':') + 3;
            int loppu = lista[i].LastIndexOf('"'); 
            lista[i] = sb.ToString(alku, loppu - alku);
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
            Console.WriteLine();
        }
    }


    public static void TulostaAjat(List<DateTime> lista)
    {
        List<string> ajat = new();
        foreach(DateTime dt in lista)
        {
            string aika = dt.ToShortDateString();
            ajat.Add(aika);
        }
        Tulosta(ajat);
    }


    /// <summary>
    /// Tulostaa annetun listan rivi kerrallaan.
    /// </summary>
    /// <param name="rivit">Lista.</param>
    public static void Tulosta(List<string> rivit)
    {
        foreach (string rivi in rivit) Console.WriteLine(rivi);
    }
}
