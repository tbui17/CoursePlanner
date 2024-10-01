import { decorate, withTryCatch } from '../util/decorate'

export function encode(data: Parameters<typeof Buffer.from>[0]): string {
  return Buffer.from(data).toString('base64')
}

export function decode(data: string): string {
  return Buffer.from(data, 'base64').toString('utf-8')
}
