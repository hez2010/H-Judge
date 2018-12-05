const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const VueLoaderPlugin = require('vue-loader/lib/plugin');
const MinifyPlugin = require('babel-minify-webpack-plugin');
const bundleOutputDir = './wwwroot/dist';

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    return [{
        mode: isDevBuild ? 'development' : 'production',
        stats: { modules: false },
        context: __dirname,
        resolve: { extensions: ['.js', '.jsx', '.vue'] },
        entry: { 'main': ['@babel/polyfill', './ClientApp/boot.js'] },
        module: {
            rules: [
                { test: /\.vue$/, include: /ClientApp/, loader: 'vue-loader', options: { loaders: { js: { loader: 'babel-loader', options: { presets: [['@babel/preset-env']], plugins: ['@babel/plugin-syntax-dynamic-import'] } } } } },
                { test: /\.jsx?$/, include: /ClientApp/, loader: 'babel-loader', options: { presets: [['@babel/preset-env']], plugins: ['@babel/plugin-syntax-dynamic-import'] } },
                { test: /\.css$/, use: isDevBuild ? ['style-loader', 'css-loader'] : [{ loader: MiniCssExtractPlugin.loader }, 'css-loader?minimize'] },
                { test: /\.(png|jpg|jpeg|gif|svg|ttf|woff|woff2|eot)$/, use: 'url-loader?limit=25000' }
            ]
        },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            chunkFilename: '[name].js',
            publicPath: isDevBuild ? '/dist/' : 'https://cdn.hjudge.com/hjudge/dist/'
        },
        plugins: [
            new MiniCssExtractPlugin({
                // Options similar to the same options in webpackOptions.output
                // both options are optional
                filename: '[name].css',
                chunkFilename: '[id].css'
            }),
            new VueLoaderPlugin()
        ].concat(isDevBuild ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
                filename: '[file].map', // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(bundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            })
        ] : [new MinifyPlugin({}, { include: /ClientApp/, sourceMap: false, babel: require('@babel/core') })])
    }];
};
