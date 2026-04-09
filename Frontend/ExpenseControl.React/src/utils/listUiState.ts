export const shouldShowHeaderCreateButton = (isLoading: boolean, itemCount: number): boolean => {
  return isLoading || itemCount > 0;
};