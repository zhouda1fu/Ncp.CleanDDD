/* eslint-env node */
require('@rushstack/eslint-patch/modern-module-resolution')

module.exports = {
  root: true,
  extends: [
    'plugin:vue/vue3-essential',
    'eslint:recommended',
    '@vue/eslint-config-typescript',
    '@vue/eslint-config-prettier/skip-formatting'
  ],
  parserOptions: {
    ecmaVersion: 'latest'
  },
  rules: {
    // Vue 3 最佳实践
    'vue/multi-word-component-names': 'off',
    'vue/component-definition-name-casing': ['error', 'PascalCase'],
    'vue/component-name-in-template-casing': ['error', 'PascalCase'],
    'vue/define-emits-declaration': 'error',
    'vue/define-props-declaration': 'error',
    'vue/no-setup-props-destructure': 'error',
    'vue/prefer-import-from-vue': 'error',
    'vue/prefer-separate-static-class': 'error',
    
    // TypeScript 最佳实践
    '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
    '@typescript-eslint/no-explicit-any': 'warn',
    '@typescript-eslint/prefer-const': 'error',
    '@typescript-eslint/no-non-null-assertion': 'warn',
    
    // 通用最佳实践
    'prefer-const': 'error',
    'no-var': 'error',
    'object-shorthand': 'error',
    'prefer-template': 'error',
    'no-console': process.env.NODE_ENV === 'production' ? 'warn' : 'off',
    'no-debugger': process.env.NODE_ENV === 'production' ? 'error' : 'off',
    
    // 代码质量
    'consistent-return': 'error',
    'no-duplicate-imports': 'error',
    'no-self-compare': 'error',
    'prefer-arrow-callback': 'error'
  }
}
