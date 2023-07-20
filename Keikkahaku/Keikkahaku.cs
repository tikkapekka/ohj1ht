using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;                    //TODO: selvitä tuota using-listaa, älytön määrä kaikkea
using System.Transactions;
using System.Globalization;
using System.Collections;
using System.Text.RegularExpressions;
using System.Security.AccessControl;               //TODO: se HttpClient aiheuttaa käännösvirheen, selvitä mitä referenssejä puuttuu

/// @author Pekka Tikkanen
/// @version 18.07.2023

/// <summary>
/// Etsitään keikkakalenterista keikat päivämäärän perusteella. //TODO: jos ehtii niin myös paikkakunnan
/// </summary>
public class Keikkahaku
{

    public static void Main()  //TODO: mieti kysytäänkö silmukassa ja tuleeko args[] jotain
    {
        string url = @"https://www.keikkakunkku.fi/keikat/kaikki-keikat/";
        string sivu = LueNetista(url);
        int alku = sivu.IndexOf("startDate") - 100;
        int loppu = sivu.IndexOf("Pidätämme");
        string[] rivit = sivu[alku..loppu].Split('\n');
        var pvm = LuePaivamaara("s");
        string[] keikat = HaeKeikat(rivit, pvm); //TODO: jos ei löydy niin kysyy uudestaan
        Tulosta(keikat);
    }


    /// <summary>
    /// Kysyy käyttäjältä päivämäärän ja palauttaa päivämäärän
    /// parametrina annetussa formaatissa.
    /// Jos päivämäärää ei löydy, käytetään tätä päivää.
    /// </summary>
    /// <param name="format">Päiväyksen formaatti.</param>
    /// <returns>Päivämäärä annetussa formaatissa.</returns>
    public static string LuePaivamaara(string format)           //TODO: 1) Toteuta niin että voi antaa myös aikavälin
    {                                                           //TODO: 2) tee TryParseExact ja tarvittavat asetukset nii löytää esim. 12.8.
        int lev = Console.LargestWindowWidth;
        int kork = Console.LargestWindowHeight;
        Console.SetWindowSize(lev, kork);
        Console.Write("Anna päivämäärä > ");
        string paivamaara = Console.ReadLine();
        DateTime pvm = new();
        string uusipvm = DateTime.TryParse(paivamaara, out pvm) ? pvm.ToString(format) : DateTime.Now.ToString(format);
        uusipvm = uusipvm[..10];
        return uusipvm;
    }


    /// <summary>
    /// Etsii sivulta kaikki keikat.
    /// </summary>
    /// <param name="sivu">Käsiteltävä merkkijono.</param>
    /// <returns>Päivämäärät, esiintyjät, keikkapaikat ja paikkakunnat
    /// kaksiulotteisena taulukkona.</returns>
    private static string[,] TaulukoiKaikkiKeikat(string[] rivit)
    {
        List<string> lista1 = new();
        string haku1 = "        \"startD";                      //TODO: Hakusanat taulukkoon ja ainakin kaksi ekaa kuormittamalla
        for (int i = 0; i < rivit.Length - 1; i++)              //TODO: Whitespacen määrä jotenkin siistimmin niin saa tuon nelosenkin samalla
        {
            if (rivit[i].StartsWith(haku1)) lista1.Add(rivit[i]);
        }

        List<string> lista2 = new();
        string haku2 = "        \"name";

        for (int i = 0; i < rivit.Length - 1; i++)
        {
            if (rivit[i].StartsWith(haku2)) lista2.Add(rivit[i]);
        }

        List<string> lista3 = new();
        string haku3 = "            \"name";

        for (int i = 0, j = 0; i < rivit.Length - 1; i++)
        {
            if (rivit[i].StartsWith(haku3)) j++;
            if (j > 0 && j % 2 == 0 && rivit[i].StartsWith(haku3)) lista3.Add(rivit[i]);    //TODO: tän voi tehdä  jälkikäteen silmukan ulkopuolella
        }  

        List<string> lista4 = new();
        string haku4 = "addressL";

        for (int i = 0; i < rivit.Length - 1; i++)
        {
            if (rivit[i].Contains(haku4)) lista4.Add(rivit[i]);
        }

        List<List<string>> listat = new() { lista1, lista2, lista3, lista4 };
      
        return TeeTaulu(listat);
    }


    /// <summary>
    /// Tekee merkkijonolistoista kaksiulotteisen taulukon.
    /// <param name="listat"> Taulukoitavat listat </param>
    /// <returns> Merkkijonot taulukkona. </returns>
    //TODO: Tähän testit, pitäis olla helppo
    public static string[,] TeeTaulu(List<List<string>> listat)
    {
        string[,] taulu = new string[listat[0].Count, listat.Count];

        for (int i = 0; i < listat[0].Count; i++)
        {
            for (int j = 0; j < listat.Count; j++)
            {
                taulu[i, j] = listat[j][i];
            }
        }
        return taulu;
    }


    /// <summary>
    /// Hakee tietyn päivämäärän keikat taulukkon.
    /// </summary>
    /// <param name="rivit"> Merkkijonotaulukko josta haetaan. </param>
    /// <param name="pvm"> Päivämäärä jolta haetaan. </param>
    /// <returns> Tulostusvalmis taulukko. </returns>
    private static string[] HaeKeikat(string[] rivit, string pvm)
    {
        string[,] taulu = TaulukoiKaikkiKeikat(rivit);
        List<string> keikkalista = new();
        for (int i = 0; i < taulu.GetLength(0); i++)
        {
            if (taulu[i, 0].Contains(pvm))
            {
                for (int j = 0; j < taulu.GetLength(1); j++)
                {
                    taulu[i, j] = Siivoa(taulu[i, j]);      //TODO: kerää pisimpien jonojen pituudet taulukkoon. 
                }
                taulu[i, 0] = TeePaivays(taulu[i, 0]);
                                                        //TODO: StringBuilder Siivoa-kohdan taulukolla ja String.PadRight-metodilla niin ei tule vakioita
                string keikka = String.Format("{0,-15} {1,-50} {2,-60} {3, -20}",  
                            taulu[i, 0], taulu[i, 1], taulu[i, 2], taulu[i, 3]);   
                
                keikkalista.Add(keikka);
            }
        }
       string[] keikatTaulukkona = keikkalista.ToArray();
       Array.Sort(keikatTaulukkona);
       return keikatTaulukkona;
    }


    /// <summary>
    /// Poistaa merkkijonosta ylimääräiset merkit ja ainakin
    /// osan "pakomerkeistä".
    /// Oletuksena on että jono sisältää tiettyjä merkkejä.  //TODO: Onko? Jos saa toimivan regexin
    /// </summary>
    /// <param name="jono">Käsiteltävä merkkijono.</param>
    /// <returns>Merkkijono siistittynä.</returns>
    private static string Siivoa(string jono)
    {
        StringBuilder sb = new();
        sb = sb.Append(jono);
        int alku = jono.IndexOf(':') + 3;          //TODO: Regexillä nämä
        int loppu = jono.LastIndexOf('"');
        jono = sb.ToString(alku, loppu - alku);
        return Regex.Unescape(jono);            //TODO: &-alkuiset ja ;-alkuiset HTML-pakomerkitt jollain pois
    }

    
    public static string LueNetista(string url)
    {
        var client = new HttpClient();
        string content = client.GetStringAsync(url).GetAwaiter().GetResult();
        return content;
    }


    /// <summary>
    /// Tulostaa taulukossa olevat merkkijonot.
    /// </summary>
    /// <param name="rivit">Tulostettava taulukko</param>
    /// <summary>
    public static void Tulosta(string[] rivit)
    {
        foreach (string rivi in rivit)
        {
            Console.WriteLine(rivi);
        }
    }


    /// <summary>
    /// Muuttaa päivämäärämerkkijonon luettavampaan muotoon.
    /// </summary>
    /// <param name="jono">Päivämäärän sisältävä merkkijono.</param>
    /// <returns>Päiväys, edessä viikonpäivä lyhyessä muodossa. Jos
    /// muunnos ei onnistu, palauttaa alkuperäisen jonon.</returns>
    public static string TeePaivays(string jono)
    {
        CultureInfo culture = CultureInfo.CreateSpecificCulture("fi-FI");
        DateTimeFormatInfo info = culture.DateTimeFormat;
        info.AbbreviatedDayNames = new string[] { "Su", "Ma", "Ti", "Ke", "To", "Pe", "La" };
        DateTime paiva = new();
        if (DateTime.TryParse(jono, out paiva)) jono = paiva.ToString("ddd dd/MM/yyyy", culture);
        return jono;
    }


}