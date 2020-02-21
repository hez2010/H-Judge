/**
 * Load a script dynamically, and execute callback after loading
 * @param name id for a script, same id won't be loaded twice
 * @param src uri
 * @param callback callback
 */
export function ensureLoading(name: string, src: string, callback: any) {
  if (document.querySelector('#script_loaded_' + name)) {
    if (callback) callback();
  }
  else {
    let script = document.createElement('script');
    script.id = 'script_loaded_' + name;
    script.type = 'text/javascript';
    script.onload = callback;
    script.src = src;
    document.body.appendChild(script);
  }
}