/// <binding BeforeBuild='copy:p6.sports:areas' Clean='clean' />
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

gulp.task("clean", ["clean:js", "clean:css", "clean:areas"]);

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

gulp.task('copy:p6.sports:areas', function () {
    return gulp.src(['../p6.sports/Areas/**', '!../p6.sports/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});
gulp.task('copy:p6.animals:areas', function () {
    return gulp.src(['../p6.animals/Areas/**', '!../p6.animals/Areas/*/{Controllers,Controllers/**}'])
        .pipe(gulp.dest('Areas/'));
});
gulp.task('watch', function () {
    gulp.watch(['../p6.sports/Areas/**'], ['copy:p6.sports:areas']);
    gulp.watch(['../p6.animals/Areas/**'], ['copy:p6.animals:areas']);
});


gulp.task("min", ["min:js", "min:css"]);
