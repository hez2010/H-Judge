export function ensureLoading(name, src, callback) {
    if (document.getElementById('script_loaded_' + name)) callback();
    else {
        let script = document.createElement('script');
        script.id = 'script_loaded_' + name;
        script.type = 'text/javascript';
        script.src = src;
        document.body.appendChild(script);
        script.onload = callback;
    }
}