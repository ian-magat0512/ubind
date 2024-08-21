var pathSegments = location.pathname.split("/");
var productId = pathSegments[1];
var environment = pathSegments[2];
var apiOrigin = location.origin;
if (productId != null && environment != null) {
    document.body.innerHTML += '  <app id="app" productId="' + productId + '" environment="' + environment + '" apiOrigin="' + apiOrigin + '"></app>';
} else {
    document.getElementById('message').style.display = 'block';
}