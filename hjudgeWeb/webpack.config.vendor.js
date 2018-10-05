const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const VueLoaderPlugin = require('vue-loader/lib/plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    return [{
        mode: isDevBuild ? 'development' : 'production',
        stats: { modules: false },
        resolve: { extensions: ['.js', '.jsx', '.vue'] },
        entry: {
            vendor: [
                'vuetify',
                'vuetify/dist/vuetify.min.css',
                'material-design-icons-iconfont/dist/material-design-icons.css',
                'event-source-polyfill',
                'vue',
                'vue-router'
            ],
        },
        module: {
            rules: [
                { test: /\.css(\?|$)/, use: [{ loader: MiniCssExtractPlugin.loader }, isDevBuild ? 'css-loader' : 'css-loader?minimize'] },
                { test: /\.jsx?$/, include: /ClientApp/, loader: 'babel-loader', options: { presets: ['@babel/preset-env'] } },
                { test: /\.(png|jpg|jpeg|gif|svg|ttf|woff|woff2|eot)(\?|$)/, use: 'url-loader?limit=100000' },
                { test: /\.vue(\?|$)/, use: 'vue-loader' }
            ]
        },
        output: {
            path: path.join(__dirname, 'wwwroot', 'dist'),
            publicPath: 'dist/',
            filename: '[name].js',
            library: '[name]_[hash]'
        },
        plugins: [
            new MiniCssExtractPlugin({
                // Options similar to the same options in webpackOptions.output
                // both options are optional
                filename: '[name].css',
                chunkFilename: '[id].css'
            }),
            new VueLoaderPlugin()
        ],
        optimization: {
            minimizer: []
                .concat(isDevBuild ? [] : [
                    new UglifyJsPlugin({
                        cache: true,
                        include: /ClientApp/,
                        parallel: true,
                        sourceMap: false
                    })]
                )
        }
    }];
};
