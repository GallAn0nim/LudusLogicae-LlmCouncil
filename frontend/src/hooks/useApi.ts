import { useState, useCallback, useRef } from 'react';

interface UseApiState<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}

interface UseApiReturn<T, A extends unknown[]> extends UseApiState<T> {
  execute: (...args: A) => Promise<T | null>;
  reset: () => void;
  setData: (data: T | null) => void;
}

export function useApi<T, A extends unknown[] = []>(
  apiFunction: (...args: A) => Promise<T>
): UseApiReturn<T, A> {
  const [state, setState] = useState<UseApiState<T>>({
    data: null,
    loading: false,
    error: null,
  });

  const apiFunctionRef = useRef(apiFunction);
  apiFunctionRef.current = apiFunction;

  const requestIdRef = useRef(0);
  const inFlightRef = useRef(false);
  const lastArgsRef = useRef<string | null>(null);

  const execute = useCallback(async (...args: A): Promise<T | null> => {
    // Deduplicate identical concurrent requests (handles React StrictMode double-calls)
    const argsKey = JSON.stringify(args);
    if (inFlightRef.current && lastArgsRef.current === argsKey) {
      return null;
    }

    inFlightRef.current = true;
    lastArgsRef.current = argsKey;
    const currentRequestId = ++requestIdRef.current;
    setState((prev) => ({ ...prev, loading: true, error: null }));

    try {
      const result = await apiFunctionRef.current(...args);
      if (currentRequestId === requestIdRef.current) {
        setState({ data: result, loading: false, error: null });
      }
      return result;
    } catch (err) {
      if (currentRequestId === requestIdRef.current) {
        const errorMessage = err instanceof Error ? err.message : 'An error occurred';
        setState({ data: null, loading: false, error: errorMessage });
      }
      return null;
    } finally {
      if (currentRequestId === requestIdRef.current) {
        inFlightRef.current = false;
      }
    }
  }, []);

  const reset = useCallback(() => {
    setState({ data: null, loading: false, error: null });
    lastArgsRef.current = null;
  }, []);

  const setData = useCallback((data: T | null) => {
    setState((prev) => ({ ...prev, data }));
  }, []);

  return { ...state, execute, reset, setData };
}

export function useApiMutation<T, A extends unknown[] = []>(
  apiFunction: (...args: A) => Promise<T>,
  options?: {
    onSuccess?: (data: T) => void;
    onError?: (error: string) => void;
  }
): UseApiReturn<T, A> {
  const [state, setState] = useState<UseApiState<T>>({
    data: null,
    loading: false,
    error: null,
  });

  const apiFunctionRef = useRef(apiFunction);
  apiFunctionRef.current = apiFunction;

  const optionsRef = useRef(options);
  optionsRef.current = options;

  const inFlightRef = useRef(false);

  const execute = useCallback(async (...args: A): Promise<T | null> => {
    // Prevent duplicate mutations while one is in flight
    if (inFlightRef.current) {
      return null;
    }

    inFlightRef.current = true;
    setState((prev) => ({ ...prev, loading: true, error: null }));

    try {
      const result = await apiFunctionRef.current(...args);
      setState({ data: result, loading: false, error: null });
      optionsRef.current?.onSuccess?.(result);
      return result;
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An error occurred';
      setState({ data: null, loading: false, error: errorMessage });
      optionsRef.current?.onError?.(errorMessage);
      return null;
    } finally {
      inFlightRef.current = false;
    }
  }, []);

  const reset = useCallback(() => {
    setState({ data: null, loading: false, error: null });
  }, []);

  const setData = useCallback((data: T | null) => {
    setState((prev) => ({ ...prev, data }));
  }, []);

  return { ...state, execute, reset, setData };
}
