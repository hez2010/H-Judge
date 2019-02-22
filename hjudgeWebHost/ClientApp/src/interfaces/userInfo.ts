export interface UserInfo {
  userId: string,
  userName: string,
  privilege: number,
  name: string,
  email: string,
  phoneNumber: string,
  isEmailConfirmed: boolean,
  isPhoneNumberConfirmed: boolean,
  otherInfo?: OtherInfo[]
}

export interface OtherInfo {
  key: string,
  description: string,
  value: string,
  canModify: boolean
}