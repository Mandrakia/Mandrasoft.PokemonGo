[MandraSoft API for PokemonGo](https://github.com/Mandrakia/Mandrasoft.PokemonGo)[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://paypal.me/MandraSoft)
=====


This solution is composed multiple projects

[TOC]

----------
Scanner
-------------
The scanner allows you to contribute to the [website](#website)
Binaries for the scanner can be downloaded [here](http://pokemongo.mandrasoft.fr/releases/Pokescanner.zip)

All you have to do is edit the .config file with any text editor and customize it.
```
    <add key="WebUri" value="http://pokemongo.mandrasoft.fr/" />
    <add key="Login" value="login" />
    <add key="Password" value="password" />
    <add key="AuthType" value="PTC" />
    <add key="BoundsToScan" value="48.899352, 2.260842, 48.818289, 2.454376" /> <!-- 2 points to make a Rectangle (Lat,lnt,lat,lng)-->
    <add key="JobsToLaunch" value="50" /> //How many Jobs are running simultaneously
    <add key="WebDelay" value="15"/> //Delay between each message to our server.
```
PokemonGo Map[^pokemongomap] Exporter
-------------
This tool allows you to export your PokemonGo Map[^pokemongomap]  database to a website.
Binaries for the scanner can be downloaded [here](http://pokemongo.mandrasoft.fr/releases/PokemonGoExporter.zip)

[Website](http://pokemongo.mandrasoft.fr/)
-------------

The website allows you to view in realtime Wild Pokemons that users submitted via the Scanner as well a as a [heatmap](http://pokemongo.mandrasoft.fr/heatmap.html) analysis of the encounters

![Real time display of pokemons](http://i.imgur.com/fcFXnef.png)
![Heatmap analysis](http://i.imgur.com/ERp0yXW.jpg)
 [^pokemongomap]: [https://github.com/AHAAAAAAA/PokemonGo-Map](https://github.com/AHAAAAAAA/PokemonGo-Map)

----------
API
-------------
The API allows you to communicate with the PokemonGo servers and do pretty much everything the server allows.
Here's a simple use of the client.

```
using (var client = new PokemonGoClient(48.8441589993527, 2.36343582639852))
{
   await client.Login();
   await client.SetServer();
   await client.UpdateInventory();
   await client.UpdateMapObjects();
   var allPokemonsOwned = client.InventoryManager.Items.Where(x => x.InventoryItemData.PokedexEntry?.TimesCaptured > 0).Select(x => x.InventoryItemData.PokedexEntry.PokedexEntryNumber);
}
```

Bot
----------

It's more of a testing ground for the API, but it can work as is as a great farming bot.
Only have to edit the config file (same as the scanner pretty much)

It can communicate with the website to know if there are pokemons you don't have in a reachable radius and go there for example which is a nice trick ;)