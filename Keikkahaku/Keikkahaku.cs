using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Transactions;
using System.Globalization;
using System.Collections;
using System.Text.RegularExpressions;
using System.Net.Http;

/// @author Pekka Tikkanen
/// @version 27.07.2023

/// <summary>
/// Konsoliohjelma, joka etsii Keikkakunkku-keikkakalenterista
/// keikat päivämäärän perusteella. Ohjelma tulostaa päivän,
/// artistin, esiintymispaikan ja paikkakunnan taulukkona.
/// </summary>
public class Keikkahaku
{    
    /// <summary>
    /// Haetaan ja tulostetaan keikat käyttäjän
    /// antamalta päivämäärältä, kunnes käyttäjä syöttää
    /// tyhjän merkkijonon. Tulosterivi esim.
    /// "Lauantai 19.08.2023 Kaija Koo Olympiastadion Helsinki"
    /// </summary>
    public static void Main()
    {
        try
        {
            string url = @"https://www.keikkakunkku.fi/keikat/kaikki-keikat/";
            string sivu = LueNetista(url);
            int alku = sivu.IndexOf("startDate") - 100;
            int loppu = sivu.IndexOf("Pidätämme");
            sivu = sivu.Substring(alku, loppu - alku);
            string[] rivit = sivu.Split('\n');
            while (true)
            {
                var pvm = LuePaivamaara("s");
                if (pvm.Length < 1) break;
                string[] keikat = HaeKeikat(rivit, pvm);
                Tulosta(keikat);
            }
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("Jokin meni pieleen.");
        }
    }


    /// <summary>
    /// Kysyy käyttäjältä päivämäärää ja yrittää muuntaa
    /// annetun merkkijonon parametrina annettuun
    /// DateTime-muotoon. Jos syötteestä ei saada luettua
    /// päivämäärää, käytetään tätä päivää. Jätetään kellonajan
    /// ilmaiseva osa pois.
    /// </summary>
    /// <param name="format"> Pyydetty päiväyksen formaatti. </param>
    /// <returns> Päivämäärä merkkijonona. </returns>
    public static string LuePaivamaara(string format)
    {
        Console.Write("Anna päivämäärä >");
        var paivamaara = Console.ReadLine();
        if (String.IsNullOrEmpty(paivamaara)) return "";
        DateTime pvm = new();
        CultureInfo culture = KulttuuriFI();
        string[] f = new string[] { "dd.MM", "dd.MM.", "dd.M", "dd.M.", 
                                    "d.M", "d.M.", "dd M", "dd MM",
                                    "d M", "dd/MM", "d/M" };
        DateTimeStyles tyyli = DateTimeStyles.AllowWhiteSpaces;
        string uusipvm = DateTime.TryParseExact(
                         paivamaara, f, culture, tyyli, out pvm)
                         ? pvm.ToString(format)
                         : DateTime.Now.ToString(format);
        uusipvm = uusipvm.Substring(0, 10);
        return uusipvm;
    }


    /// <summary>
    /// Etsii sivulta kaikki keikat.
    /// </summary>
    /// <param name="sivu">Käsiteltävä merkkijono.</param>
    /// <returns> Päivämäärät, esiintyjät, keikkapaikat ja paikkakunnat
    /// kaksiulotteisena taulukkona.</returns>
    private static string[,] TaulukoiKaikkiKeikat(string[] rivit)
    {
        string[] haut = new string[]{
                                     "        \"startD",
                                     "        \"name",
                                     "            \"name",
                                     "                \"addressLoc" };
        var listat = new List<List<string>>();
        for (int i = 0; i < haut.Length; i++) listat.Add(new List<string>());
        int lkm = 0;
        for (int i = 0; i < rivit.Length; i++)
        {
            for (int j = 0; j < haut.Length; j++)
            {   //otetaan tämän sisennystason
                // "name"-alkuisista riveistä joka toinen:
                if (j == 2)
                {
                    if (rivit[i].StartsWith(haut[j]))
                    {
                        lkm++;
                        if (lkm > 0 && lkm % 2 == 0)
                        {
                            listat[j].Add(rivit[i]);
                        }
                    }
                }
                else if (rivit[i].StartsWith(haut[j]))
                {
                     listat[j].Add(rivit[i]);
                }
            }
        }
        return TeeTaulu(listat);
    }


    /// <summary>
    /// Tekee merkkijonolistoista kaksiulotteisen taulukon.
    /// Listoissa pitää olla yhtä monta alkiota.
    /// <param name="listat"> Taulukoitavat listat. </param>
    /// <returns> Merkkijonot taulukkona. </returns>
    /// <example>
    /// <pre name="test">
    /// List<List<string>> listat = new(){ new List<string> { "Yö", "Dingo", "Popeda" },
    ///                                    new List<string> { "Pori", "myös Pori", "Tampere" }};
    /// string[,] taulu = TeeTaulu(listat);
    /// taulu[2,1] === "Tampere";
    /// taulu === new string[,]{{ "Yö", "Pori" }, { "Dingo", "myös Pori" }, { "Popeda", "Tampere" }};
    /// List<List<string>> listat2 = new(){ new List<string> { "A", "B", "C", "D" },
    ///                                     new List<string> { "E", "F", "G", "H" },
    ///                                     new List<string> { "I", "J", "K", "L" }};
    /// string[,] taulu2 = TeeTaulu(listat2);
    /// taulu2 === new string[,]{{ "A", "E", "I" }, { "B", "F", "J" }, { "C", "G", "K" }, { "D", "H", "L" }};
    /// listat2.Add(new List<string> { "1", "2" });
    /// TeeTaulu(listat2); #THROWS System.ArgumentOutOfRangeException
    /// </pre>
    /// </example>
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


     // TODO: Sarakkeiden leveydet parametrina pisimmän merkkijonon mukaan.
    /// <summary>
    /// Taulukoi haetun päivämäärän keikat ja järjestää ne aakkosjärjestykseen.
    /// </summary>
    /// <param name="rivit"> Merkkijonotaulukko josta haetaan. </param>
    /// <param name="pvm"> Päivämäärä jolta haetaan. </param>
    /// <returns> Tulostusvalmis taulukko. </returns>
    private static string[] HaeKeikat(string[] rivit, string pvm)
    {
        string[,] t = TaulukoiKaikkiKeikat(rivit);
        List<string> keikkalista = new();
        for (int i = 0; i < t.GetLength(0); i++)
        {
            if (t[i, 0].Contains(pvm))
            {
                for (int j = 0; j < t.GetLength(1); j++)
                {
                    t[i, j] = Siivoa(t[i, j]);
                }
                t[i, 0] = TeePaivays(t[i, 0]);
                string keikka = String.Format(
                                             "{0,-15} {1,-50} {2,-60} {3, -20}",
                                              t[i, 0], t[i, 1], t[i, 2], t[i, 3]);

                keikkalista.Add(keikka);
            }
        }
        string[] keikatTaulukkona = keikkalista.ToArray();
        Array.Sort(keikatTaulukkona);
        return keikatTaulukkona;
    }


    /// <summary>
    /// Poistaa merkkijonosta ylimääräiset merkit ja korjaa
    /// ainakin suurimman osan "pakomerkeistä".
    /// </summary>
    /// <param name="jono"> Käsiteltävä merkkijono.</param>
    /// <returns> Merkkijono siistittynä.</returns>
    private static string Siivoa(string jono)
    {
        StringBuilder sb = new();
        sb = sb.Append(jono);
        int alku = jono.IndexOf(':') + 3;
        int loppu = jono.LastIndexOf('"');
        jono = sb.ToString(alku, loppu - alku);
        //Ääkköset saadaan korjattua tällä.
        jono = Regex.Unescape(jono);
        //Korjataan pari hankalampaa erikoismerkkiä käsin.                     
        jono = Regex.Replace(jono, "\\&#8217;", "'");
        jono = Regex.Replace(jono, "\\&#038;", "&");
        jono = Regex.Replace(jono, "\\&#8211\\S{1,}", "—");
        return jono;
    }


     // TODO: kääntäjä nurisee tästä, korvaa uuden-
     // aikaisemmalla tavalla kunhan opit
    /// <summary>
    /// Lähettää osoitteen palvelimelle pyynnön ja palauttaa
    /// sivun tiedot merkkijonona. Heittää poikkeuksen, jos
    /// pyyntöä ei voida täyttää.
    /// </summary>
    /// <param name="url"> Sivun url-osoite. </param>
    /// <returns> Sivun HTML-koodi. </returns>
    public static string LueNetista(string url)
    {
        HttpClient client = new();
        string sivu = client.GetStringAsync(url).GetAwaiter().GetResult();
        return sivu;
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
        string viiva = new string('=', 158);
        Console.WriteLine(viiva);
    }


    /// <summary>
    /// Muuttaa päivämäärämerkkijonon luettavampaan muotoon.
    /// </summary>
    /// <param name="jono"> Päivämäärän sisältävä merkkijono. </param>
    /// <returns> Päiväys, jossa edessä viikonpäivä. Jos
    /// muunnos ei onnistu, palautetaan alkuperäinen jono.</returns>
    public static string TeePaivays(string jono)
    {
        CultureInfo culture = KulttuuriFI();
        DateTime paiva = new();
        if (DateTime.TryParse(jono, out paiva)) 
        {
            jono = paiva.ToString("dddd dd/MM/yyyy", culture);
        }
        return jono;
    }


    /// <summary>
    /// Luodaan päivämäärän kulttuuriasetukset ja
    /// lisätään suomenkieliset viikonpäivät.
    /// </summary>
    /// <returns> Asetukset. </returns>
    public static CultureInfo KulttuuriFI()
    {
        CultureInfo culture = CultureInfo.CreateSpecificCulture("fi-FI");
        DateTimeFormatInfo info = culture.DateTimeFormat;
        info.AbbreviatedDayNames = new string[] { "Su", "Ma", "Ti", "Ke",
                                                  "To", "Pe", "La" };
        info.DayNames = new string[] { "Sunnuntai", "Maanantai", "Tiistai",
                                       "Keskiviikko", "Torstai",
                                       "Perjantai", "Lauantai" };
        return culture;
    }


}