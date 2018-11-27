export function initializeObjects(obj, env) {
    for (let i in obj) {
        env[i] = obj[i];
    }
}
