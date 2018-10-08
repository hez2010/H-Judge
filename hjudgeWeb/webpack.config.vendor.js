const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    return [{
        mode: isDevBuild ? 'development' : 'production',
        stats: { modules: false },
        resolve: { extensions: ['.js', '.jsx'] },
        entry: {
            vendor: [
                'vuetify',
                'event-source-polyfill',
                'highlight.js',
                'vue',
                'vue-router',
                'isomorphic-fetch'
            ]
        },
        module: {
            rules: [
                { test: /\.css$/, use: [{ loader: MiniCssExtractPlugin.loader }, isDevBuild ? 'css-loader' : 'css-loader?minimize'] },
                { test: /\.jsx?$/, include: /ClientApp/, loader: 'babel-loader', options: { presets: [['@babel/preset-env', { targets: { ie: '11' } }]] } },
                { test: /\.(png|jpg|jpeg|gif|svg|ttf|woff|woff2|eot)(\?|$)/, use: 'url-loader?limit=100000' }
            ]
        },
        output: {
            path: path.join(__dirname, 'wwwroot', 'dist'),
            publicPath: '/dist/',
            filename: '[name].js',
            library: '[name]_[hash]'
        },
        plugins: [
            new MiniCssExtractPlugin({
                // Options similar to the same options in webpackOptions.output
                // both options are optional
                filename: '[name].css',
                chunkFilename: '[id].css'
            })
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
