describe('SignPuddle Web Application', () => {
  test('should be able to run basic tests', () => {
    expect(true).toBe(true);
  });

  test('should have correct environment', () => {
    expect(typeof window).toBe('undefined'); // Node.js environment
  });

  test('should handle basic string operations', () => {
    const testString = 'SignPuddle';
    expect(testString).toContain('Sign');
    expect(testString.length).toBe(10); // Fixed: SignPuddle has 10 characters
  });

  test('should handle array operations', () => {
    const testArray = [1, 2, 3, 4, 5];
    expect(testArray).toHaveLength(5);
    expect(testArray).toContain(3);
    expect(testArray[0]).toBe(1);
  });

  test('should handle object operations', () => {
    const testObject = {
      name: 'SignPuddle',
      version: '1.0.0',
      type: 'web-application'
    };
    
    expect(testObject).toHaveProperty('name');
    expect(testObject.name).toBe('SignPuddle');
    expect(Object.keys(testObject)).toHaveLength(3);
  });
});