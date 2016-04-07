/// <binding Clean='clean' />
"use strict";

var gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    uglify = require("gulp-uglify"),
    gulpFilter = require('gulp-filter');

var paths = {
    webroot: "./wwwroot/"
};

paths.js = paths.webroot + "js/**/*.js";
paths.minJs = paths.webroot + "js/**/*.min.js";
paths.css = paths.webroot + "css/**/*.css";
paths.minCss = paths.webroot + "css/**/*.min.css";
paths.concatJsDest = paths.webroot + "js/site.min.js";
paths.concatCssDest = paths.webroot + "css/site.min.css";
paths.areas = "Areas";

gulp.task("clean:js", function (cb) {
    rimraf(paths.concatJsDest, cb);
});

gulp.task("clean:css", function (cb) {
    rimraf(paths.concatCssDest, cb);
});

gulp.task("clean", ["clean:js", "clean:css"]);

gulp.task("min:js", function () {
    return gulp.src([paths.js, "!" + paths.minJs], { base: "." })
        .pipe(concat(paths.concatJsDest))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task("min:css", function () {
    return gulp.src([paths.css, "!" + paths.minCss])
        .pipe(concat(paths.concatCssDest))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

gulp.task("clean:areas", function (cb) {
    rimraf(paths.areas, cb);
});

gulp.task('copy:P6.Main.IdentityServerHost', function () {
    return gulp.src(['../P6.Main.IdentityServerHost/Views/**'])
        .pipe(gulp.dest('Views/'));
});

gulp.task('copy:P6.Main.IdentityServerHost:areas', function () {
    return gulp.src(['../P6.Main.IdentityServerHost/Areas/**', '!../P6.Main.IdentityServerHost/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});


gulp.task('copy:p6.PartnerManagement:assets', function () {
    return gulp.src(['../p6.PartnerManagement/assets/**'])
        .pipe(gulp.dest(paths.webroot + 'assets/'));
});

gulp.task('copy:p6.PartnerManagement:areas', function () {
    return gulp.src(['../p6.PartnerManagement/Areas/**', '!../p6.PartnerManagement/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});

gulp.task('copy:DeveloperAuth', function () {
    return gulp.src(['../DeveloperAuth/Areas/**', '!../DeveloperAuth/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});

gulp.task('copy:Pingo.Authorization:areas', function () {
    return gulp.src(['../Pingo.Authorization/Areas/**', '!../Pingo.Authorization/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});



gulp.task('watch', [
        'copy:P6.Main.IdentityServerHost',
        'copy:P6.Main.IdentityServerHost:areas',
        'copy:p6.PartnerManagement:assets',
        'copy:p6.PartnerManagement:areas',
        'copy:Pingo.Authorization:areas',
        'copy:DeveloperAuth'
],
    function () {
        gulp.watch(['../P6.Main.IdentityServerHost/Views/**'], ['copy:P6.Main.IdentityServerHost']);
        gulp.watch(['../P6.Main.IdentityServerHost/Areas/**'], ['copy:P6.Main.IdentityServerHost:areas']);
        gulp.watch(['../p6.PartnerManagement/assets/**'], ['copy:p6.PartnerManagement:assets']);
        gulp.watch(['../p6.PartnerManagement/Areas/**'], ['copy:p6.PartnerManagement:areas']);
        gulp.watch(['../DeveloperAuth/Areas/**'], ['copy:DeveloperAuth']);
        gulp.watch(['../Pingo.Authorization/Areas/**'], ['copy:Pingo.Authorization:areas']);

    });

gulp.task("min", ["min:js", "min:css"]);
