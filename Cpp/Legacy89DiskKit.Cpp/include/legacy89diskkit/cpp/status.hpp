#pragma once

#include <string>
#include <utility>

namespace legacy89diskkit::cpp
{
enum class StatusCode
{
    Ok = 0,
    InvalidArgument,
    UnsupportedFormat,
    ParseError,
    OutOfRange
};

struct Status
{
    StatusCode code;
    std::string message;

    [[nodiscard]] bool ok() const
    {
        return code == StatusCode::Ok;
    }

    static Status OkStatus()
    {
        return {StatusCode::Ok, {}};
    }
};

template <typename TValue>
class Result
{
public:
    static Result Success(TValue value)
    {
        return Result(Status::OkStatus(), std::move(value));
    }

    static Result Failure(StatusCode code, std::string message)
    {
        return Result(Status{code, std::move(message)}, TValue{});
    }

    [[nodiscard]] bool ok() const
    {
        return status_.ok();
    }

    [[nodiscard]] const Status& status() const
    {
        return status_;
    }

    [[nodiscard]] const TValue& value() const
    {
        return value_;
    }

    [[nodiscard]] TValue& value()
    {
        return value_;
    }

private:
    Result(Status status, TValue value)
        : status_(std::move(status)), value_(std::move(value))
    {
    }

    Status status_;
    TValue value_;
};
}
