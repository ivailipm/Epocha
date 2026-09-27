export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

// The API returns either { detail: "..." } (a hand-written Problem response) or ASP.NET Core's
// automatic { errors: { Field: ["message"] } } shape for validation failures.
async function extractErrorMessage(response: Response): Promise<string> {
  const problem = await response.json().catch(() => null)

  if (typeof problem?.detail === 'string') {
    return problem.detail
  }

  if (problem?.errors && typeof problem.errors === 'object') {
    const messages = Object.values(problem.errors).flat()
    if (messages.length > 0) {
      return messages.join(' ')
    }
  }

  return 'Something went wrong. Please try again.'
}

export async function throwIfNotOk(response: Response): Promise<void> {
  if (!response.ok) {
    throw new ApiError(response.status, await extractErrorMessage(response))
  }
}
