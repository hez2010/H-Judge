/**
 * Pick a state from global states
 * @param obj
 */
export function getTargetState<T>(obj: [any, any]): [T, ((arg: T) => void)] {
  return [obj[0], obj[1]];
}