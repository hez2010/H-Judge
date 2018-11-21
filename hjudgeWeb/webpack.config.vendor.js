const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const MinifyPlugin = require('babel-minify-webpack-plugin');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    return [{
        mode: isDevBuild ? 'development' : 'production',
        stats: { modules: false },
        resolve: { extensions: ['.js', '.jsx'] },
        entry: {
            vendor: [
                'event-source-polyfill'
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
            }),
            new MinifyPlugin({}, { include: /ClientApp/, sourceMap: false, babel: require('@babel/core') })
        ]
    }];
};
