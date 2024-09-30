/**
 * Unit tests for the action's entrypoint, src/index.ts
 */

import * as main from '../src/main'
import { expect, vi, describe, it } from 'vitest'

// Mock the action's entrypoint
const runMock = vi.spyOn(main, 'run').mockImplementation(async () => {})

describe('index', () => {
  it('calls run when imported', async () => {
    await import('../src/index')

    expect(runMock).toHaveBeenCalled()
  })
})
