export function isTeacher(privilege: number) {
  return privilege >= 1 && privilege <= 3;
}

export function isAdmin(privilege: number) {
  return privilege === 1;
}