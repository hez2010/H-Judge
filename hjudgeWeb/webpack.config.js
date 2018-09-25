const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const VueLoaderPlugin = require('vue-loader/lib/plugin');
const bundleOutputDir = './wwwroot/dist';
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    return [{
        stats: { modules: false },
        context: __dirname,
        resolve: { extensions: ['.js', '.jsx', '.vue'] },
        entry: { 'main': './ClientApp/boot.js' },
        module: {
            rules: [
                { test: /\.vue$/, include: /ClientApp/, loader: 'vue-loader', options: { loaders: { js: 'babel-loader' } } },
                { test: /\.jsx?$/, include: /ClientApp/, loader: 'babel-loader', options: { presets: ['@babel/preset-env'] } },
                { test: /\.css$/, use: isDevBuild ? ['style-loader', 'css-loader'] : [{ loader: MiniCssExtractPlugin.loader }, 'css-loader'] },
                { test: /\.(png|jpg|jpeg|gif|svg|ttf|woff|woff2|eot)$/, use: 'url-loader?limit=25000' }
            ]
        },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: 'dist/'
        },
        plugins: [
            new MiniCssExtractPlugin({
                // Options similar to the same options in webpackOptions.output
                // both options are optional
                filename: '[name].css',
                chunkFilename: '[id].css'
            }),
            new VueLoaderPlugin(),
            new webpack.DefinePlugin({
                'process.env': {
                    NODE_ENV: JSON.stringify(isDevBuild ? 'development' : 'production')
                }
            }),
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require('./wwwroot/dist/vendor-manifest.json')
            })
        ].concat(isDevBuild ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
                filename: '[file].map', // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(bundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            })
        ] : []),
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
