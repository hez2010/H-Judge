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
  experience: number,
  coins: number,
  otherInfo: OtherInfo[]
}

export interface OtherInfo {
  key: string,
  name: string,
  value: string
}