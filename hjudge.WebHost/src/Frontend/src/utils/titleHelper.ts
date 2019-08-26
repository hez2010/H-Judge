export function setTitle(title: string) {
  const isSSR = typeof window === 'undefined';
  if (!isSSR) document.title = title + ' - H::Judge';
}