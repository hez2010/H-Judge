export function ensureLoading(name: string, src: string, callback: any) {
  if (document.querySelector('#script_loaded_' + name)) {
    if (callback) callback();
  }
  else {
    let script = document.createElement('script');
    script.id = 'script_loaded_' + name;
    script.type = 'text/javascript';
    script.src = src;
    document.body.appendChild(script);
    script.onload = callback;
  }
}