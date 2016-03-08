/*jslint browser: true, devel: true, node: true, ass: true, nomen: true, unparam: true, indent: 4 */

/**
 * @license
 * Copyright (c) 2015 The ExpandJS authors. All rights reserved.
 * This code may only be used under the BSD style license found at https://expandjs.github.io/LICENSE.txt
 * The complete set of authors may be found at https://expandjs.github.io/AUTHORS.txt
 * The complete set of contributors may be found at https://expandjs.github.io/CONTRIBUTORS.txt
 */
(function (global) {
    "use strict";

    // Vars
    var parser   = require('path-to-regexp'),
        XP       = global.XP || require('expandjs'),
        director = XP.isBrowser() ? require('director/build/director') : require('director'),
        Router   = XP.isBrowser() ? director.Router : director.http.Router;

    /*********************************************************************/

    /**
     * A class used to perform client/server side routing.
     *
     * @class XPRouter
     * @description A class used to perform client/server side routing
     */
    module.exports = global.XPRouter = new XP.Class('XPRouter', {

        /**
         * @constructs
         * @param {Object} [server]
         */
        initialize: function (server) {

            // Vars
            var self = this;

            // Setting
            self._adaptee = new Router();
            self._server  = XP.isBrowser() ? null : server;

            // Configuring
            self._adaptee.configure({recurse: false});

            // Listening
            if (self._server) { self._server.on('request', self._handleRequest.bind(self)); }
        },

        /*********************************************************************/

        /**
         * TODO DOC
         *
         * @method on
         * @param {string} path
         * @param {string} [method = "GET"]
         * @param {Function} callback
         * @returns {Object}
         */
        on: function (path, method, callback) {

            // Preparing
            if (XP.isFunction(method)) { callback = method; method = 'GET'; }

            // Asserting
            XP.assertArgument(XP.isString(path, true), 1, 'string');
            XP.assertArgument(XP.isString(method, true), 2, 'string');
            XP.assertArgument(XP.isFunction(callback), 3, 'Function');

            // Vars
            var self = this,
                keys = [];

            // Checking
            if (self.running) { return self; }

            // Overriding
            keys   = XP.pluck(parser(path, keys) && keys, 'name');
            method = XP.isBrowser() ? 'on' : method.toLowerCase();

            // Listening
            if (XP.isBrowser()) {
                self._adaptee[method](path, function () { callback(XP.zipObject(keys, arguments)); });
            } else {
                self._adaptee[method](path, function () { callback(XP.assign(this.req, {params: XP.zipObject(keys, arguments)}), this.res); });
            }

            return self;
        },

        /**
         * TODO DOC
         *
         * @method run
         * @returns {Object}
         */
        run: function () {

            // Vars
            var self = this;

            // Initializing
            if (!self.running && XP.isBrowser()) { XP.delay(function () { self._adaptee.init('/'); }); }

            // Setting
            return self;
        },

        /*********************************************************************/

        /**
         * TODO DOC
         *
         * @property route
         * @type Array
         * @readonly
         */
        route: {
            get: function () { return (this.running && this._adaptee.getRoute()) || null; }
        },

        /**
         * TODO DOC
         *
         * @property running
         * @type boolean
         * @readonly
         */
        running: {
            set: function (val) { return this.running || !!val; }
        },

        /*********************************************************************/

        /**
         * TODO DOC
         *
         * @property _adaptee
         * @type Object
         * @private
         */
        _adaptee: {
            enumerable: false,
            set: function (val) { return this._adaptee || val; },
            validate: function (val) { return !XP.isObject(val) && 'Object'; }
        },

        /**
         * TODO DOC
         *
         * @property _server
         * @type Object
         * @private
         */
        _server: {
            enumerable: false,
            set: function (val) { return XP.isDefined(this._server) ? this._server : val; },
            validate: function (val) { return !XP.isBrowser() && !XP.isObject(val) && 'Object'; }
        },

        /*********************************************************************/

        // HANDLER
        _handleRequest: function (req, res) {

            // Vars
            var self   = this,
                chunks = [];

            // Listening
            req.on('data', function (chunk) { chunks.push(chunk); });
            req.on('end', function () { req.body = XP.join(chunks); });

            // Dispatching
            self._adaptee.dispatch(req, res, function () {
                res.writeHead(404);
                res.end();
            });
        }
    });

}(typeof window !== "undefined" ? window : global));
