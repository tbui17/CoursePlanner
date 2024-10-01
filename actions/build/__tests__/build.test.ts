import { vi, describe, beforeEach, expect, it } from 'vitest'
import * as b64 from '../src/encoding/b64'
import * as R from 'remeda'

function nameof<T>(obj: T, key: keyof T) {
  return key
}

describe('b64', () => {
  const testData = {
    type: 'TYPE',
    project_id: 'PROJECT_ID',
    private_key_id: 'PRIVATE_KEY_ID',
    private_key:
      '-----BEGIN PRIVATE KEY-----\nPRIVATE_KEY\n-----END PRIVATE KEY-----\n',
    client_email: 'CLIENT_EMAIL',
    client_id: 'CLIENT_ID',
    auth_uri: 'AUTH_URI',
    token_uri: 'TOKEN_URI',
    auth_provider_x509_cert_url: 'AUTH_PROVIDER_x509_CERT_URL',
    client_x509_cert_url: 'CLIENT_x509_CERT_URL',
    universe_domain: 'UNIVERSE_DOMAIN'
  } as const

  const privateKeys = [
    {
      ...testData,
      private_key:
        '\n-----BEGIN PRIVATE KEY-----\nPRIVATE_KEY\n-----END PRIVATE KEY-----\n'
    },
    {
      ...testData,
      private_key:
        '-----BEGIN PRIVATE KEY-----\nPRIVATE_KEY\n-----END PRIVATE KEY-----\n'
    },
    {
      ...testData,
      private_key:
        '-----BEGIN PRIVATE KEY----- PRIVATE_KEY\n-----END PRIVATE KEY-----\n'
    },
    {
      ...testData,
      private_key:
        '-----BEGIN PRIVATE KEY-----PRIVATE_KEY-----END PRIVATE KEY-----\n'
    },
    {
      ...testData,
      private_key:
        '-----BEGIN PRIVATE KEY-----PRIVATE_KEY-----END PRIVATE KEY-----'
    },

    {
      ...testData,
      private_key:
        '-----BEGIN PRIVATE KEY-----\nPRIVATE_KEY\n-----END PRIVATE KEY-----'
    },
    {
      ...testData,
      private_key:
        '\n-----BEGIN PRIVATE KEY-----PRIVATE_KEY-----END PRIVATE KEY-----\n'
    },

    {
      ...testData,
      private_key:
        '\n-----BEGIN PRIVATE KEY-----\nPRIVATE_KEY-----END PRIVATE KEY-----'
    }
  ]

  const rnPrivateKeys = privateKeys.map(x => ({
    ...x,
    private_key: x.private_key.replace(/\n/g, '\r\n')
  }))

  const inputs = [...privateKeys, ...rnPrivateKeys]

  function encodeDecode(input: string) {
    const encoded = b64.encode(input)
    return b64.decode(encoded)
  }

  describe('should not throw', () => {
    it.each(inputs)(`$${nameof(testData, 'private_key')}`, input => {
      const s = JSON.stringify(input)
      expect(() => encodeDecode(s)).not.toThrow()
    })
  })

  describe(`should keep integrity of whitespace and new lines`, () => {
    it.each(inputs)(`$${nameof(testData, 'private_key')}`, input => {
      const strData = JSON.stringify(input)
      expect(encodeDecode(strData)).toBe(strData)
    })
  })

  describe('should be JSON parseable', () => {
    it.each(inputs)(`$${nameof(testData, 'private_key')}`, input => {
      const strData = JSON.stringify(input)
      const s1 = encodeDecode(strData)
      const json = JSON.parse(s1)
      expect(json).toEqual(input)
    })
  })
})
