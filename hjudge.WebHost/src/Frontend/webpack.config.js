const path = require('path');
const fs = require('fs');
const { CleanWebpackPlugin } = require('clean-webpack-plugin');

module.exports = function (webpackEnv) {
  const appDirectory = fs.realpathSync(process.cwd());
  const resolveApp = relativePath => path.resolve(appDirectory, relativePath);
  const appSrc = resolveApp('src');

  const isEnvProduction = webpackEnv === 'production';

  return {
    mode: isEnvProduction ? 'production' : 'development',
    bail: isEnvProduction,
    entry: {
      main: './src/index.tsx'
    },
    devServer: {
      contentBase: path.join(__dirname, 'public'),
      compress: true,
      port: 3000,
      inline: true,
      hot: true
    },
    output: {
      path: path.join(__dirname, 'build'),
      filename: '[name].bundle.js',
      globalObject: 'this',
      publicPath: '/dist/'
    },
    resolve: {
      extensions: ['.ts', '.tsx', '.js']
    },
    optimization: {
      splitChunks: {
        chunks: 'all'
      }
    },
    module: {
      rules: [
        {
          test: /\.(js|jsx|ts|tsx)$/,
          include: appSrc,
          use: [
            {
              loader: 'babel-loader',
              options: {
                customize: require.resolve(
                  'babel-preset-react-app/webpack-overrides'
                ),
                cacheDirectory: true,
                cacheCompression: isEnvProduction,
                compact: isEnvProduction,
              }
            },
            'astroturf/loader'
          ],
        },
        {
          test: /\.(s*)css$/,
          use: [
            'isomorphic-style-loader',
            {
              loader: 'css-loader',
              options: {
                importLoaders: 1
              }
            },
            'postcss-loader'
          ]
        },
        { test: /\.woff(\?.+)?$/, use: 'url-loader?limit=10000&mimetype=application/font-woff' },
        { test: /\.woff2(\?.+)?$/, use: 'url-loader?limit=10000&mimetype=application/font-woff' },
        { test: /\.ttf(\?.+)?$/, use: 'file-loader' },
        { test: /\.eot(\?.+)?$/, use: 'file-loader' },
        { test: /\.svg(\?.+)?$/, use: 'file-loader' },
        { test: /\.png$/, use: 'url-loader?mimetype=image/png' },
        { test: /\.gif$/, use: 'url-loader?mimetype=image/gif' }
      ]
    },
    plugins: [
      new CleanWebpackPlugin()
    ]
  }
}