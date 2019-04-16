let excludeSegments = [
  'download',
  'static'
];

self.addEventListener('fetch', function (event) {
  let url = event.request.url.toLowerCase();
  for (let x in excludeSegments) {
    if (url.indexOf(`/${excludeSegments[x]}/`) !== -1) return false;
  }
});