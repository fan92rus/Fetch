module.exports = {
  root: true,

  parserOptions: {
    parser: "babel-eslint",
    sourceType: "module"
  },

  env: {
    browser: true
  },
  extends: ["plugin:vue/essential", "eslint:recommended", "@vue/typescript"],
  // required to lint *.vue files
  plugins: ["vue", "import"],

  globals: {
    ga: true, // Google Analytics
    cordova: true,
    __statics: true,
    process: true,
    Capacitor: true
  },

  // add your custom rules here
  rules: {
    "class-methods-use-this": "off",
    "no-param-reassign": "off",
    "object-shorthand": "off",
    "space-before-function-paren": "off",
    "import/first": "off",
    "import/named": "error",
    "import/namespace": "error",
    "import/default": "error",
    "import/export": "error",
    "import/extensions": "off",
    "import/no-unresolved": "off",
    "import/no-extraneous-dependencies": "off",
    "import/prefer-default-export": "off",
    "prefer-promise-reject-errors": "off",
    "comma-dangle": "off",
    "prefer-destructuring": "off",
    "prefer-template": "off",
    quotes: "off",
    "max-len": [2, 260, 4, { ignoreUrls: true }],
    "linebreak-style": "off",
    indent: "off",
    "operator-linebreak": "off",
    "arrow-parens": "off",
    "no-console": "off",
    "await-in-loop": "off",
    // allow console.log during development only
    "no-console": process.env.NODE_ENV === "production" ? "error" : "off",
    // allow debugger during development only
    "no-debugger": process.env.NODE_ENV === "production" ? "error" : "off"
  }
};
