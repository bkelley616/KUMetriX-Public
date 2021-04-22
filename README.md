# KUMetriX

A stock data web app built using the [Safe Stack](https://safe-stack.github.io/) with the [Feliz SAFE Template](https://github.com/Zaid-Ajaj/SAFE.Simplified) that features:

<ul>
<li>Basic Authentication / Profile Storage (Azure hosted MongoDB)</li>
<li>Login Sessions implemented with GUID Session ID's and DateTime Expiration</li>
<li>Argon2id Password Hashing (<a href="https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html">OWASP Standard</a>)</li>
<li> <a href="https://elmish.github.io/elmish/">Elmish</a> MVU Single Page Application (built on top of React)</li>
<li>Supports Time Travel Debugging (<a href="https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd?hl=en">Redux DevTools</a>)</li>
<li>Persistent State with Local Storage </li>
<li>Secrets/Environment variables using config.json</li>
</ul>
<br>

## MongoDB Profile Structure:

<img align="center" src="./docs/assets/DatabaseObject.png" alt="Database Object" width="400"/>
<br><br>

## Video Demo:

<br><br>

## Set up:

- Download / Clone Repo
- Set up [Azure CosmosDB](https://azure.microsoft.com/en-us/services/cosmos-db/) (choose MongoDB)
- Set up [MarketStack Account](https://marketstack.com/) and create API Key
- Install Software Dependencies
- Add config.json to server directory
- Start Server & Client
  <br><br>

### Software Dependencies

- [.NET Core SDK 3.1+](https://dotnet.microsoft.com/download)
- [Node.js 12.0+](https://nodejs.org/en)
  <br><br>

### Config:

Add config.json to server directory

<pre><code>{
  "DBCONNECTIONSTRING": "MongoDB connection string",
  "INACTIVESESSIONID": "00000000-0000-0000-0000-000000000000",
  "INACTIVESESSIONEXPIRATION": "0001-01-01T00:00:00.0000000",
  "DBNAME": "MongoDB Name",
  "COLLECTIONNAME": "MongoDB Collection Name",
  "PREFIXSALT": "Salt String",
  "SUFFIXSALT": "Salt String",
  "EXTERNALSTOCKAPIKEY": "Market Stack API key",
  "STOCKAPIBASEURL": "http://api.marketstack.com/v1/eod",
  "CSUSIGNUPCODE": "Sign Up String",
  "PROGSIGNUPCODE": "Sign Up String",
  "GENERICSIGNUPCODE": "Sign Up String"
}
</code></pre>
<br>

### Server:

fetch server packages and run

<pre><code>cd server
dotnet restore
dotnet run
</code></pre>

Served from: http://localhost:5000
<br><br>

### Client:

fetch client packages and run

<pre><code>cd client
npm install
npm start
</code></pre>

Served from: http://localhost:8080 (Webpack forwards requests to server port)
<br><br>

### Deployment:

Run Prod build and output is sent to {solutionRoot}/dist
Deploy with [Azure Web Apps](https://azure.microsoft.com/en-us/services/app-service/web/)

<pre><code>dotnet run
 _______    _       ___  ____   ________
|_  __  |  / \     |_  ||_  _| |_   __  |
 | |_ \_| / _ \      | |_/ /     | |_ \_|
 |  _|   / ___ \     |  __'.     |  _| _
_| |_  _/ /   \ \_  _| |  \ \_  _| |__/ |
|____||____| |____||____||____||________| 

[Interactive Mode] Run build target: Pack
run Pack
</code></pre>

More info at [Template Repo](https://github.com/Zaid-Ajaj/SAFE.Simplified)
<br><br>
