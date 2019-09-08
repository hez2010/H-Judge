const padding = (num: number, padding: number) => {
    let str = num.toString();
    if (str.length >= padding) return str;
    return '0'.repeat(padding - str.length) + str;
}

export function toInputDateTime(time: Date) {
    return `${padding(time.getFullYear(), 4)}-${padding(time.getMonth() + 1, 2)}-${padding(time.getDate(), 2)}T${padding(time.getHours(), 2)}:${padding(time.getMinutes(), 2)}:${padding(time.getSeconds(), 2)}`;
}