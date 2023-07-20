# Harjoitustyön suunnitelma


## Tietoja 

Tekijä: Pekka Tikkanen

Työ git-varaston osoite: <https://github.com/tikkapekka/ohj1ht.git>

Ohjelman nimi: Keikkahaku

Alusta: Windows/macOS/Linux


## Tietoja ohjelmasta

 * Ohjelman nimi: Keikkahaku
 * Alusta: Windows/macOS/Linux

## Ohjelman idea ja tavoitteet
Netistä löytyvässä keikkakalenterissa

 * <https://www.keikkakunkku.fi/keikat/kaikki-keikat/>

on paljon ylimääräistä mainostekstiä. Ohjelma kerää olennaisimmat tiedot eli
päivämäärän, esiintyjän, keikkapaikan ja paikkakunnan yhteen tekstitaulukkoon.

## Hahmotelma ohjelmasta
Ohjelma käynnistetään komentoriviltä ja se kysyy käyttäjältä päivämäärän.
Keikkakunkun sivun HTML-muodosta
>
>    {
>        "@context": "http://schema.org",
>        "@type": "Event",
>        "name": "Kaija Koo",
>        "startDate": "2023-08-19T18:00:00",
>        "description": "KAIJA KOO \u2013 SUPERSTADION. Helsinki, Olympiastadion\r\nLa 19.8.2023. Ovet klo 18.00. Kyseess\u00e4 on Kaijan t\u00e4m\u00e4n vuoden > > > ainoa >konsertti. \u1d20\u1d07\u0280\u028f \ua731\u1d18\u1d07\u1d04\u026a\u1d00\u029f \u0262\u1d1c\u1d07\ua731\u1d1b \u1d0b\u00c4\u00c4\u0280\u026a\u1d0a\u00c4!",
>        "performers": {
>            "@type": "Organization",
>            "name": "Kaija Koo",
>            "url": "https://www.keikkakunkku.fi/kaija-koo/"
>        },
>        "location": {
>            "@type": "Place",
>            "name": "Olympiastadion",
>            "address": {
>                "@type": "PostalAddress",
>                "streetAddress": "Paavo Nurmen tie 1",
>                "addressLocality": "Helsinki",
>                "postalCode": "00250",
>                "addressCountry": "FI"
>            }
>        },
>        "offers": {
>            "@type": "Offer",
>            "price": ".49,90",
>            "url": "https://www.lippu.fi/artist/kaija-koo/"
>        }

etsitään lähdekoodin indentaatiotasoja hyödyntävillä hakulauseilla rivit joilla
on meitä kiinnostavaa tietoa. Sen jälkeen haetaan annetulla päivämäärällä olevat
keikat, siistitään rivit, ja kootaan tiedot taulukkoon. Lisätään vielä viikonpäivä 
päivämäärän eteen. Yllä oleva keikka tulostuisi syötteeseen muodossa

> La 19/08/2023 Kaija Koo Olympiastadion Helsinki



## Toteutuksen suunnitelma

vk 26

- projektin luonti
- tuumailua

vk 27–28

- rakenteen suunnittelu paperilla
- tiedostojen lukeminen 
- aliohjelmakutsut
- aliohjelmien toteutusta
- testaamista

vk 29–30

- aliohjelmien toteutus jatkuu
- testaaminen jatkuu
- poikkeusten käsittely
- kaikki nätiksi


Jos aikaa jää

- haku paikkakunnan perusteella
- laajentamismahdollisuuksien pohdinta