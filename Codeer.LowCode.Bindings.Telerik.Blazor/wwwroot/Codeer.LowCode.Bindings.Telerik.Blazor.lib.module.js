/**
 *
 * @param {string} src
 * @returns {void}
 */
function installScript(src) {
  const script = document.createElement("script");
  script.setAttribute("src", src);
  document.head.appendChild(script);
}

/**
 *
 * @param {string} src
 * @returns {void}
 */
function installCss(src) {
  const link = document.createElement("link");
  link.setAttribute("rel", "stylesheet");
  link.setAttribute("href", src);
  document.head.appendChild(link);
}

function startup() {
  installCss("_content/Telerik.UI.for.Blazor.Trial/css/kendo-theme-default/all.css");
  installScript("_content/Telerik.UI.for.Blazor.Trial/js/telerik-blazor.js");
}

export function beforeStart() {
  startup();
}

export function beforeWebStart() {
  startup();
}
