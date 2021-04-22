namespace HelpersUtil

open Fable.SimpleHttp
open Thoth.Json

open Autocomplete

module TypedCSS =
  open Zanaptak.TypedCssClasses
  type Bulma = CssClasses<"../node_modules/bulma/css/bulma.css", Naming.Underscores> 
  type Icon = CssClasses<"../node_modules/@fortawesome/fontawesome-free/css/all.css", Naming.Underscores> 
  
module Images =
  let kumetrixLogo() = StaticFile.import "./imgs/logos/KUMetriX-Logo.svg"
  let kumetrixAltLogo() = StaticFile.import "./imgs/logos/KUMetriXAlt-Logo.svg"
  let kumetrixNavLogo() = StaticFile.import "./imgs/logos/KUMetriX-Nav-Logo.svg"
  let notFoundDog() = StaticFile.import "./imgs/notfound/not-found-pic.png"

