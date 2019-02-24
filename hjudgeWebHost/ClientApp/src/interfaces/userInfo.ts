export interface UserInfo {
  userId: string,
  userName: string,
  privilege: number,
  name: string,
  signedIn: boolean,
  email: string,
  phoneNumber: string,
  emailConfirmed: boolean,
  phoneNumberConfirmed: boolean,
  otherInfo?: OtherInfo[]
}

export interface OtherInfo {
  key: string,
  description: string,
  value: string,
  canModify: boolean
}